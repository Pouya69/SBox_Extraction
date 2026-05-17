using Sandbox;

public sealed class Elevator : Component, IPobxMover
{
	[Property, Feature("Doors")] public IDoor ElevatorDoor { get; private set; }

	/// <summary>
	/// The actual stop positions. (Amount of floors) of AnimatingGameObject.
	/// if AnimatingGameObject is empty, uses the position of this Game Object.
	/// </summary>
	[Property, Feature( "Elevator" )] private List<Vector3> ElevatorStopPositions { get; set; } = new();

	/// <summary>
	/// The target that we are moving to. For starting it should be at where the current floor is.
	/// </summary>
	[Property, Feature( "Elevator" )] private int TargetElevatorPositionIndex { get; set; } = 0;

	/// <summary>
	/// For the actual elevator going up down. Defaults to self GameObject if empty.
	/// </summary>
	[Property, Feature( "Elevator" )] private GameObject AnimatingGameObject { get; set; }
	[Property, Feature( "Elevator" ), DefaultValue( 8.0f )] private float ElevatorMoveSpeed { get; set; } = 8.0f;

	[Property, Feature( "Elevator" ), DefaultValue( 1.0f )] private float WaitAfterCloseDoorsBeforeMoving = 1.0f;
	[Property, Feature( "Elevator" ), DefaultValue( 1.0f )] private float WaitAfterArrivingBeforeOpenDoors = 1.0f;

	private int _previousElevatorPositionIndex;
	private bool _isMoving = false;
	Vector3 _currentTargetPos;
	private bool _willMove = false;

	[Property, Group( "Events" )] public Doo OnStartedMoving { get; set; }
	[Property, Group( "Events" )] public Doo OnReachedTargetFloor { get; set; }

	[Property, Group( "Sounds" )] public SoundEvent StartedMovingSound { get; set; }
	[Property, Group( "Sounds" )] public SoundEvent MovingSound { get; set; }
	[Property, Group( "Sounds" )] public SoundEvent StoppedMovingSound { get; set; }

	SoundHandle _movingSoundHandle;

	public bool IsMoving() => _isMoving;
	public int AmountOfFloors => ElevatorStopPositions.Count;

	public bool AreDoorsInProgress => ElevatorDoor.IsInProgress();

	public void StartMoving()
	{
		_willMove = false;

		if ( StartedMovingSound != null )
			AnimatingGameObject.PlaySound( StartedMovingSound );

		if (MovingSound != null )
		{
			_movingSoundHandle = AnimatingGameObject.PlaySound( MovingSound );
		}

		_isMoving = true;
		this.Enabled = true;
	}

	/// <summary>
	/// Used for elevators that have 1 button for example.
	/// </summary>
	public void GoToNextFloor()
	{
		Log.Info( $"Previous Floor: {_previousElevatorPositionIndex+1}, Next Floor: {_previousElevatorPositionIndex + 2}" );
		GoToFloorNumber( _previousElevatorPositionIndex+2 <= AmountOfFloors ? _previousElevatorPositionIndex + 2 : 1 );
	}

	public void GoToPreviousFloor()
	{
		Log.Info( $"Previous Floor: {_previousElevatorPositionIndex + 1}, Next Floor: {_previousElevatorPositionIndex}" );
		GoToFloorNumber( _previousElevatorPositionIndex >= 1 ? _previousElevatorPositionIndex : AmountOfFloors );
	}

	public void GoToFloorNumber(int floorNumber) {
		Log.Info( $"Trying to go to floor {floorNumber}" );

		// When moving we don't change it.
		if ( IsMoving() || AreDoorsInProgress )
			return;

		if (floorNumber > AmountOfFloors )
		{
			Log.Error( $"Elevator {GameObject.Name} has less than requested {floorNumber} floor number." );
			return;
		}
		if (floorNumber <= 0)
		{
			Log.Error( $"Elevator {GameObject.Name} got {floorNumber} floor number (not index. Floor numbers)." );
			return;
		}

		TargetElevatorPositionIndex = floorNumber-1;
		_currentTargetPos = ElevatorStopPositions[TargetElevatorPositionIndex];

		if ( IsAlreadyAtTargetFloor() )
		{
			StartOpeningDoors();
			return;
		}

		StartClosingDoors();
	}

	public bool IsAlreadyAtTargetFloor() => AnimatingGameObject.LocalPosition.DistanceSquared( _currentTargetPos ) <= 1.0f;

	public void StopMoving()
	{
		_movingSoundHandle?.Stop();

		_previousElevatorPositionIndex = TargetElevatorPositionIndex;
		_isMoving = false;
		this.Enabled = false;
	}

	protected override void OnFixedUpdate()
	{
		if ( !_isMoving || AreDoorsInProgress )
			return;

		if ( IsAlreadyAtTargetFloor() )
		{
			// Reached Destination.
			AnimatingGameObject.LocalPosition = _currentTargetPos;
			ReachedDestination();
			return;
		}

		AnimatingGameObject.LocalPosition = MathUtils.VInterpConstantTo( AnimatingGameObject.LocalPosition, _currentTargetPos, Time.Delta, ElevatorMoveSpeed );
	}

	private async void ReachedDestination()
	{
		if ( StoppedMovingSound != null )
			AnimatingGameObject.PlaySound( StoppedMovingSound );

		StopMoving();

		Run( OnReachedTargetFloor, delegate ( Doo.Configure c ) {
			c.SetArgument( "floor", TargetElevatorPositionIndex );
		} );

		await Task.DelaySeconds(WaitAfterArrivingBeforeOpenDoors);

		StartOpeningDoors();
	}

	public void StartOpeningDoors()
	{
		if (!IsMoving())
			ElevatorDoor.OpenDoor();
	}

	public void StartClosingDoors()
	{
		ElevatorDoor.CloseDoor();
	}

	private async void ElevatorDoor_OnDoorClosed()
	{

		_willMove  = true;
		await Task.DelaySeconds( WaitAfterCloseDoorsBeforeMoving );
		StartMoving();
	}

	protected override void OnAwake()
	{
		if ( AmountOfFloors <= 1 )
		{
			Log.Error( $"Elevator {GameObject.Name} has less than 2 floors." );
		}
		if ( AnimatingGameObject == null )
			AnimatingGameObject = this.GameObject;

		_currentTargetPos = AnimatingGameObject.LocalPosition;

		// StartOpeningDoors();
		StopMoving();

		ElevatorDoor.OnDoorClosed += ElevatorDoor_OnDoorClosed;
	}

	protected override void OnDestroy()
	{
		ElevatorDoor.OnDoorClosed -= ElevatorDoor_OnDoorClosed;
	}

	
}
