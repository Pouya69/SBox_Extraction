using Sandbox;

public sealed class SlidingDoor : Component, IInteractable, IDoor
{

	public void Interact( PlayerInteractionComponent interactionComponent )
	{
		throw new NotImplementedException();
	}

	public bool IsInProgress()
	{
		throw new NotImplementedException();
	}

	public bool IsInteractable() => !IsInProgress();

	public bool IsOpen()
	{
		throw new NotImplementedException();
	}

	public void StartCloseDoor()
	{
		throw new NotImplementedException();
	}

	public void StartOpenDoor()
	{
		throw new NotImplementedException();
	}

	protected override void OnUpdate()
	{

	}

	public bool IsPickUpTwoHanded() => false;
	public bool CanBePickedUp() => false;
}
