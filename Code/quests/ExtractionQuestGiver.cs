using Sandbox;

public sealed class ExtractionQuestGiver : Component, IInteractable
{
	[Property, Group( "Prototype_ONLY" )] public List<QuestInfo> TESTONLY_QuestsAvailable { get; private set; } = new();

	public void Interact( PlayerInteractionComponent interactionComponent )
	{
		if ( TESTONLY_QuestsAvailable.Count == 0 ) return;

		var removedFromAvailable = TESTONLY_QuestsAvailable[0];

		var questCreated = ExtractionQuestUtility.CreateQuestFromQuestInfo( removedFromAvailable );
		TESTONLY_QuestsAvailable.RemoveAt( 0 );

		interactionComponent.PlayerQuestSystem.AddQuest( questCreated );
	}
}
