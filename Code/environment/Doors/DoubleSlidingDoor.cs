using Sandbox;

public sealed class DoubleSlidingDoor : Component, IInteractable, IDoor
{


	private Vector3 _closedPosition;
	private Vector3 _closedPosition_02;

	private Vector3 _targetPosition;
	private Vector3 _targetPosition_02;

	[Property, Feature( "Door" )] private GameObject Door_01 { get; set; }
	[Property, Feature( "Door" )] private GameObject Door_02 { get; set; }
	[Property, Feature( "Door" )] private Vector3 OpenPosition { get; set; }
	[Property, Feature( "Door" )] private Vector3 OpenPosition_02 { get; set; }
	[Property, Feature( "Door" )] private float DoorSpeed { get; set; } = 50.0f;

	private bool _isOpen = false;

	protected override void OnAwake()
	{
		_closedPosition = this.Door_01.LocalPosition;
		_closedPosition_02 = this.Door_02.LocalPosition;
		this.Enabled = false;
	}

	public void Interact( PlayerInteractionComponent interactionComponent )
	{
		if (!IsInProgress())
			ToggleOpenDoor();
	}

	public bool IsInProgress() => this.Enabled;

	public bool IsInteractable() => !IsInProgress();

	public bool IsOpen() => _isOpen;

	[Button("Toggle Door", "door_open" ), Feature("Door")]
	public void ToggleOpenDoor()
	{
		_isOpen = !_isOpen;
		_targetPosition = _isOpen ? OpenPosition : _closedPosition;
		_targetPosition_02 = _isOpen ? OpenPosition_02 : _closedPosition_02;
		this.Enabled = true;
	}

	[Button( "Open Door", "door_open" ), Feature( "Door" )]
	public void OpenDoor()
	{
		_isOpen = true;
		_targetPosition = OpenPosition;
		_targetPosition_02 = OpenPosition_02;
		this.Enabled = true;
	}

	[Button( "Close Door", "door_open" ), Feature( "Door" )]
	public void CloseDoor()
	{
		_isOpen = false;
		_targetPosition = _closedPosition;
		_targetPosition_02 = _closedPosition_02;
		this.Enabled = true;
	}

	protected override void OnFixedUpdate()
	{
		this.Door_01.LocalPosition = MathUtils.VInterpTo(this.Door_01.LocalPosition, _targetPosition, Time.Delta, DoorSpeed );
		this.Door_02.LocalPosition = MathUtils.VInterpTo( this.Door_02.LocalPosition, _targetPosition_02, Time.Delta, DoorSpeed );
		if ((this.Door_01.LocalPosition).DistanceSquared(this._targetPosition) <= 2f * 2f && (this.Door_02.LocalPosition).DistanceSquared( this._targetPosition_02 ) <= 2f * 2f)
		{
			this.Enabled = false;
		}
	}

	public bool IsPickUpTwoHanded() => false;
	public bool CanBePickedUp() => false;
}
