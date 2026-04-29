using Sandbox;
using System;
using System.Text.Json.Serialization;

public enum EQuestStatus
{
	NOT_STARTED,
	IN_PROGRESS,
	COMPLETED,
	FAILED,
}

public enum EQuestObjectiveType
{
	LOCATION,
	KILL,
	SPEAK,
	FETCH
}

public interface IExtractionQuestEntity
{
	public void AddEntityToGlobalManager();
	public bool IsAlive();

	public void EnteredArea(QuestLocationInfo location);
	public void EntityKilled( IExtractionQuestEntity Instigator );
	public void EntityPickedUp( IExtractionQuestEntity Instigator );
}

public interface IExtractionQuest
{

	public string GetQuest_GUID();
	public void QuestStarted();
	public void QuestComplete();
	public void QuestFailed();
	public void QuestUpdated();
	public bool IsQuestComplete();
	public bool IsQuestFailed();
	public bool IsQuestInProgress();
	public bool IsObjectiveComplete( QuestObjectiveInfo Objective );
	public bool IsObjectiveComplete(string Objective_UID);

	public bool ObjectiveComplete(QuestObjectiveInfo objective);
	public bool ObjectiveComplete(int objectiveIndex);
	public bool ObjectiveFailed( QuestObjectiveInfo objective );
	public bool ObjectiveFailed( int objectiveIndex );

	public QuestObjectiveInfo GetObjectiveInfo(int objectiveIndex);

	public QuestObjectiveInfo[] GetQuestObjectives();
}

public static class ExtractionQuestUtility
{

	public static ExtractionQuest CreateQuestFromQuestInfo(QuestInfo questInfo)
	{
		return new ExtractionQuest( questInfo );
	}

	public static void CheckQuestObjectiveConditions(IExtractionQuest quest, QuestObjectiveInfo objective, object objectToCheck, EQuestObjectiveCondition actionTaken, ExtractionPlayerQuestSystemHandlerComponent playerQuestSystem)
	{
		foreach ( var condition in objective.FailureConditions )
		{
			if ( condition.IsConditionMet( objectToCheck, actionTaken, playerQuestSystem ) == EQuestObjectiveResultType.NOT_RELAVANT )
				continue;

			if ( condition.WillFinishQuest )
			{
				quest.QuestFailed();
				continue;
			}
			quest.ObjectiveFailed( objective );
			return;
		}



		if ( objective.SuccessCondition.IsConditionMet( objectToCheck, actionTaken, playerQuestSystem ) == EQuestObjectiveResultType.NOT_RELAVANT )
			return;

		if ( objective.SuccessCondition.WillFinishQuest )
		{
			quest.QuestComplete();
			return;
		}
		quest.ObjectiveComplete( objective );
	}

	public static IExtractionQuest GetQuestByGUID(string QuestGUID) => ExtractionQuestSystem.GetQuestByGUID( QuestGUID );
}

public static class GuidGenerator
{
	public static string NewQuestId => "q-" + Guid.NewGuid().ToString();

	public static string NewObjectiveId => "o-" + Guid.NewGuid().ToString();

	public static string NewEntityId => "e-" + Guid.NewGuid().ToString();

	public static string NewLocationId => "l-" + Guid.NewGuid().ToString();
}
