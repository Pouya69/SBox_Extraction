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

public enum EExtractionObjectSize
{
	SMALL,
	MEDIUM,
	LARGE,
	CREATURE_TYPE
}

public interface IExtractionQuestEntity
{
	public void AddEntityToGlobalManager();
	public bool IsAlive();

	public void EnteredArea(QuestLocationInfo location);
	public void EntityKilled( IExtractionQuestEntity Instigator );
	public void EntityPickedUp( IExtractionQuestEntity Instigator );

	public string GetEntityName();

	public EExtractionObjectSize GetObjectSize();

	public bool CanBeRemoteGrabbed();

	public Renderer GetRenderer();

	public void ToggleEnablePhysics( bool enable );
	
	public void LaunchEntity(Vector3 velocity, bool ignoreMass = true);

	public GameObject GetGameObject();

	public Rigidbody GetRigidbody();
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

	public bool ObjectiveComplete(QuestObjectiveInfo objective, ExtractionPlayerQuestSystemHandlerComponent playerQuestSystem );
	public bool ObjectiveComplete(int objectiveIndex);
	public bool ObjectiveFailed( QuestObjectiveInfo objective );
	public bool ObjectiveFailed( int objectiveIndex );

	public QuestObjectiveInfo GetObjectiveInfo(int objectiveIndex);

	public QuestObjectiveInfo[] GetQuestStartingObjectives();
	public QuestObjectiveInfo[] GetQuestObjectives();
}

public static class ExtractionQuestUtility
{

	public static ExtractionQuest CreateQuestFromQuestInfo(QuestInfo questInfo)
	{
		return new ExtractionQuest( questInfo );
	}

	public static bool CheckQuestObjectiveConditions(IExtractionQuest quest, QuestObjectiveInfo objective, object objectToCheck, EQuestObjectiveCondition actionTaken, ExtractionPlayerQuestSystemHandlerComponent playerQuestSystem)
	{
		foreach ( var condition in objective.FailureConditions )
		{
			if ( condition.IsConditionMet( objectToCheck, actionTaken, playerQuestSystem ) == EQuestObjectiveResultType.NOT_RELAVANT )
				continue;

			if ( condition.WillFinishQuest )
			{
				quest.QuestFailed();
				return true;
			}
			quest.ObjectiveFailed( objective );
			return true;
		}

		foreach ( var successCondition in objective.SuccessConditions )
		{
			// This works for OR operator. for AND @TODO
			if ( successCondition.IsConditionMet( objectToCheck, actionTaken, playerQuestSystem ) == EQuestObjectiveResultType.NOT_RELAVANT )
				continue;

			if ( successCondition.WillFinishQuest )
			{
				quest.QuestComplete();
				return true;
			}

			Log.Info( "Objective Complete: " + objective.Description );
			quest.ObjectiveComplete( objective, playerQuestSystem );
			return true;
		}

		return false;
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
