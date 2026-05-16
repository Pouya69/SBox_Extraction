using Sandbox;

public sealed class SlidingDoor : Component, IInteractable, IDoor
{
	private Vector3 _closedPosition;
	private Vector3 _targetPosition;

	[Property, Feature( "Door" )] private Vector3 OpenPosition { get; set; }
	[Property, Feature( "Door" )] private float DoorSpeed { get; set; } = 50.0f;

	private bool _isOpen = false;

	protected override void OnAwake()
	{
		_closedPosition = this.WorldPosition;
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
		this.Enabled = true;
	}

	[Button( "Open Door", "door_open" ), Feature( "Door" )]
	public void OpenDoor()
	{
		_isOpen = true;
		_targetPosition = OpenPosition;
		this.Enabled = true;
	}

	[Button( "Close Door", "door_open" ), Feature( "Door" )]
	public void CloseDoor()
	{
		_isOpen = false;
		_targetPosition = _closedPosition;
		this.Enabled = true;
	}

	protected override void OnFixedUpdate()
	{
		this.WorldPosition = MathUtils.VInterpTo(this.WorldPosition, _targetPosition, Time.Delta, DoorSpeed );
		if ((this.WorldPosition).DistanceSquared(this._targetPosition) <= 2f * 2f )
		{
			this.Enabled = false;
		}
	}

	public bool IsPickUpTwoHanded() => false;
	public bool CanBePickedUp() => false;
}
