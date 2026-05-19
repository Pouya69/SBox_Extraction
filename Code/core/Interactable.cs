
public interface IInteractable
{
	public void Interact(IInteractionComp interactionComponent);

	public void Released() { }

	public bool CanBePickedUp();
	public bool IsPickUpTwoHanded();

	public bool IsInteractable();

	public int GetCost() => 0;
}
