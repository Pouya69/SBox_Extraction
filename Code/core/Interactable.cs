
public interface IInteractable
{
	public void Interact(PlayerInteractionComponent interactionComponent);
	public bool CanBePickedUp();
	public bool IsPickUpTwoHanded();

	public bool IsInteractable();
}
