using Sandbox;

public class GrabbableComponent : Component, IInteractable
{
	[Property, RequireComponent] public IExtractionQuestEntity EntityRef { get; set; }
	[Property] public bool IsGrabTwoHanded { get; protected set; }

	public bool CanBePickedUp() => true;

	public void Interact( PlayerInteractionComponent interactionComponent )
	{
		interactionComponent.PickUpEntity( EntityRef );
	}

	public bool IsInteractable() => true;

	public bool IsPickUpTwoHanded() => IsGrabTwoHanded;
}
