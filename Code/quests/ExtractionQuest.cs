using Sandbox;

public class ExtractionQuest : IExtractionQuest
{
	public ExtractionQuest(QuestInfo questInfo) { 
		QuestInfo = questInfo;
		QuestObjectives = questInfo.QuestObjectives;
	}

	private EQuestStatus QuestStatus { get; set; } = EQuestStatus.NOT_STARTED;

	[Property, ReadOnly] public QuestInfo QuestInfo { get; private set; }

	[Property, ReadOnly] public List<QuestObjectiveInfo> QuestObjectives = new();

	public QuestObjectiveInfo GetObjectiveInfo( int objectiveIndex )
	{
		if ( QuestObjectives.Count <= objectiveIndex ) return null;

		return QuestObjectives[objectiveIndex];
	}

	public QuestObjectiveInfo[] GetQuestObjectives()
	{
		return QuestObjectives.ToArray();
	}

	public string GetQuest_GUID()
	{
		return QuestInfo.Quest_UID;
	}

	public bool IsObjectiveComplete( QuestObjectiveInfo Objective )
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

	public bool ObjectiveComplete( QuestObjectiveInfo objective, ExtractionPlayerQuestSystemHandlerComponent playerQuestSystem )
	{
		playerQuestSystem.QuestObjectiveCompleted( this, objective );
		return true;
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

	public override string ToString()
	{

		string result = "Quest Title: " + QuestInfo.Title + "\n"
			+ "Quest Description: " + QuestInfo.Description + "\n"
			+ "Quest Objectives:";


		if ( QuestObjectives.Count == 0 )
			return result;

		for ( int i = 0; i < QuestObjectives.Count; i++ ) {

			var item = QuestObjectives[i];
			result += "\nObjective " + (i + 1) + ": " + item.Description + "\n\tSuccess Conditions:\n\t\t";

			for ( int j = 0; j < item.SuccessConditions.Count; j++ )
			{
				var successCondition = item.SuccessConditions[j];
				result += successCondition.ToString() + "\n\t\t";
			}
		}

		return result;
	}

	public QuestObjectiveInfo[] GetQuestStartingObjectives()
	{
		return QuestInfo.StartingObjectives.ToArray();
	}
}
