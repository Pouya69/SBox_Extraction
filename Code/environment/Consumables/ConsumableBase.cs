using Sandbox;

public class ConsumableBase : Component, IInteractable
{
	[Property] public int Cost { get; set; }

	public bool CanBePickedUp()
	{
		return false;
	}

	public virtual void Interact( IInteractionComp interactionComponent )
	{
		
	}

	public virtual void Consume() { }

	public bool IsInteractable() => true;

	public bool IsPickUpTwoHanded()
	{
		return false;
	}

	public virtual int GetCost() => Cost;
}
