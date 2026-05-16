
public interface IInteractable
{
	public void Interact(PlayerInteractionComponent interactionComponent);

	public void Released() { }

	public bool CanBePickedUp();
	public bool IsPickUpTwoHanded();

	public bool IsInteractable();
}
