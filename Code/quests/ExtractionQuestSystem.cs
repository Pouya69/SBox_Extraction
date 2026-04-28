using Sandbox;
using System;

public class ExtractionQuestSystem
{
	public event Action<IExtractionQuestEntity> OnEntityKilled;
	public event Action<IExtractionQuestEntity> OnEntityPickedUp;
	public event Action<QuestLocationInfo> OnLocationEntered;
	public event Action<QuestLocationInfo> OnLocationExited;

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

	public static void EntityKilled(IExtractionQuestEntity entityKilled) {
		Instance.OnEntityKilled?.Invoke( entityKilled );
	}

	public static void EntityPickedUp( IExtractionQuestEntity entityKilled )
	{
		Instance.OnEntityPickedUp?.Invoke( entityKilled );
	}

	public static void LocationEntered( QuestLocationInfo LocationEntered)
	{
		Instance.OnLocationEntered?.Invoke( LocationEntered );
	}

	public static void LocationExited( QuestLocationInfo LocationExited )
	{
		Instance.OnLocationExited?.Invoke( LocationExited );
	}

	public static IExtractionQuest GetQuestByGUID(string quest_guid) {
		foreach ( var quest in Instance.Quests )
			if ( quest.GetQuest_GUID().Equals( quest_guid ) ) return quest;

		return null;
	}
}

