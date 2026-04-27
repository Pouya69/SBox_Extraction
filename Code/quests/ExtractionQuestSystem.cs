using Sandbox;

public class ExtractionQuestSystem
{
	public List<ExtractionQuest> CurrentQuests { get; private set; }

	private static ExtractionQuestSystem Instance { get; set; }

	public List<IExtractionQuest> Quests { get; private set; }

	public ExtractionQuestSystem()
	{
		Instance = this;
	}

	public static void LoadQuestData()
	{

	}

	public static IExtractionQuest GetQuestByGUID(string quest_guid) {
		foreach ( var quest in Instance.Quests )
			if ( quest.GetQuest_GUID().Equals( quest_guid ) ) return quest;

		return null;
	}
}

