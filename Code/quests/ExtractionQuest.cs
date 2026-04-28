using Sandbox;

public class ExtractionQuest : IExtractionQuest
{
	private EQuestStatus QuestStatus { get; set; } = EQuestStatus.NOT_STARTED;

	[Property, ReadOnly] private QuestInfo QuestInfo { get; set; }

	[Property, ReadOnly] public List<FExtractionGenericObjective> QuestObjectives;

	[Property, ReadOnly] public List<FExtractionGenericObjective> FailedObjectives;

	[Property, ReadOnly] public List<FExtractionGenericObjective> ActiveObjectives;

	[Property, ReadOnly] public List<FExtractionGenericObjective> CompletedObjectives;

	public QuestObjectiveInfo GetObjectiveInfo( int objectiveIndex )
	{
		if ( QuestObjectives.Count <= objectiveIndex ) return null;

		return QuestObjectives[objectiveIndex].ObjectiveInfo;
	}

	public FExtractionGenericObjective[] GetQuestObjectives()
	{
		return QuestObjectives.ToArray();
	}

	public string GetQuest_GUID()
	{
		return QuestInfo.Quest_UID;
	}

	public bool IsObjectiveComplete( FExtractionGenericObjective Objective )
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

	public bool ObjectiveComplete( QuestObjectiveInfo objective )
	{
		throw new System.NotImplementedException();
	}

	public bool ObjectiveComplete( int objectiveIndex )
	{
		throw new System.NotImplementedException();
	}

	public bool ObjectiveFailed( QuestObjectiveInfo objective )
	{
		throw new System.NotImplementedException();
	}

	public bool ObjectiveFailed( int objectiveIndex )
	{
		throw new System.NotImplementedException();
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

	public FExtractionGenericObjective[] GetCurrentObjectives()
	{
		return ActiveObjectives.ToArray();
	}
}

public struct FExtractionGenericObjective
{
	public FExtractionGenericObjective( QuestObjectiveInfo objectiveInfo)
	{
		ObjectiveStatus = EQuestStatus.NOT_STARTED;
		ObjectiveInfo = objectiveInfo;
	}

	public EQuestStatus ObjectiveStatus;
	public QuestObjectiveInfo ObjectiveInfo;
}
