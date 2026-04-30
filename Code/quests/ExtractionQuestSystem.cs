using Sandbox;
using System;

public class ExtractionQuestSystem
{
	public event Action<IExtractionQuestEntity, IExtractionQuestEntity> OnEntityKilled;
	public event Action<IExtractionQuestEntity, IExtractionQuestEntity> OnEntityPickedUp;
	public event Action<IExtractionQuestEntity, QuestLocationInfo> OnLocationEntered;
	public event Action<IExtractionQuestEntity, QuestLocationInfo> OnLocationExited;

	public List<ExtractionQuest> CurrentQuests { get; private set; }

	public static ExtractionQuestSystem Instance { get; set; }

	public List<IExtractionQuest> Quests { get; private set; }

	public ExtractionQuestSystem()
	{
		Instance = this;
	}

	public static void LoadQuestData()
	{

	}

	public static void EntityKilled( IExtractionQuestEntity Instigator, IExtractionQuestEntity entityKilled) {
		Instance.OnEntityKilled?.Invoke( Instigator, entityKilled );
	}

	public static void EntityPickedUp( IExtractionQuestEntity Instigator, IExtractionQuestEntity entityKilled )
	{
		Instance.OnEntityPickedUp?.Invoke( Instigator, entityKilled );
	}

	public static void LocationEntered( IExtractionQuestEntity Instigator, QuestLocationInfo LocationEntered )
	{
		Instance.OnLocationEntered?.Invoke( Instigator, LocationEntered );
	}

	public static void LocationExited( IExtractionQuestEntity Instigator, QuestLocationInfo LocationExited )
	{
		Instance.OnLocationExited?.Invoke( Instigator, LocationExited );
	}

	public static IExtractionQuest GetQuestByGUID(string quest_guid) {
		foreach ( var quest in Instance.Quests )
			if ( quest.GetQuest_GUID().Equals( quest_guid ) ) return quest;

		return null;
	}
}

