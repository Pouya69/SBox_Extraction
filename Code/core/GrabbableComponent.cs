using Sandbox;

public sealed class GrabbableComponent : Component, IInteractable
{
	[Property, RequireComponent] public IExtractionQuestEntity EntityRef { get; set; }
	[Property] public bool IsGrabTwoHanded { get; private set; }

	public bool CanBePickedUp() => true;

	public void Interact( PlayerInteractionComponent interactionComponent )
	{
		interactionComponent.PickUpEntity( EntityRef );
	}

	public bool IsPickUpTwoHanded() => IsGrabTwoHanded;
}
