using Sandbox;

public class ConsumableBase : Component, IInteractable
{
	public bool CanBePickedUp()
	{
		return false;
	}

	public virtual void Interact( PlayerInteractionComponent interactionComponent )
	{
		
		
	}

	public virtual void Consume() { }

	public bool IsInteractable() => true;

	public bool IsPickUpTwoHanded()
	{
		return false;
	}
}
