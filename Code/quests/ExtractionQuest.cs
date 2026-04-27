using Sandbox;

public class ExtractionQuest : IExtractionQuest
{
	private EQuestStatus QuestStatus { get; set; } = EQuestStatus.NOT_STARTED;

	[Property] private QuestInfo QuestInfo { get; set; }



	public QuestObjectiveInfo[] GetQuestObjectives()
	{
		throw new System.NotImplementedException();
	}

	public string GetQuest_GUID()
	{
		return QuestInfo.Quest_UID;
	}

	public bool IsObjectiveComplete( ExtractionGenericObjective Objective )
	{
		throw new System.NotImplementedException();
	}

	public bool IsObjectiveComplete( string Objective_UID )
	{
		throw new System.NotImplementedException();
	}

	public bool IsQuestComplete()
	{
		return QuestStatus == EQuestStatus.COMPLETED;
	}

	public bool IsQuestFailed()
	{
		return QuestStatus == EQuestStatus.FAILED;
	}

	public bool IsQuestInProgress()
	{
		return QuestStatus == EQuestStatus.IN_PROGRESS;
	}

	public void QuestComplete()
	{
		QuestStatus = EQuestStatus.COMPLETED;
	}

	public void QuestFailed()
	{
		QuestStatus = EQuestStatus.FAILED;
	}

	public void QuestStarted()
	{
		QuestStatus = EQuestStatus.IN_PROGRESS;
	}

	public void QuestUpdated()
	{
		throw new System.NotImplementedException();
	}

}

public class ExtractionGenericObjective
{

}
