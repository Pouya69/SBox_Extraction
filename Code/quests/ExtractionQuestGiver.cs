using Sandbox;

public sealed class ExtractionQuestGiver : Component, IInteractable
{
	[Property, Group( "Prototype_ONLY" )] public List<QuestInfo> TESTONLY_QuestsAvailable { get; private set; } = new();

	public void Interact( IInteractionComp interactionComponent )
	{
		if ( TESTONLY_QuestsAvailable.Count == 0 ) return;
		var playerInteractionComp = interactionComponent as PlayerInteractionComponent;
		if ( !playerInteractionComp.IsValid() )
			return;

		var removedFromAvailable = TESTONLY_QuestsAvailable[0];

		var questCreated = ExtractionQuestUtility.CreateQuestFromQuestInfo( removedFromAvailable );
		TESTONLY_QuestsAvailable.RemoveAt( 0 );

		playerInteractionComp.PlayerQuestSystem.AddQuest( questCreated );
	}

	public bool IsPickUpTwoHanded() => false;
	public bool CanBePickedUp() => false;

	public bool IsInteractable() => true;
}
