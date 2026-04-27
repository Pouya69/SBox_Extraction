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

public interface IExtractionQuestEntity
{
	public void AddEntityToGlobalManager();
	public bool IsAlive();
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
	public bool IsObjectiveComplete( ExtractionGenericObjective Objective );
	public bool IsObjectiveComplete(string Objective_UID);

	public QuestObjectiveInfo[] GetQuestObjectives();
}

public static class ExtractionQuestUtility
{

	public static IExtractionQuest GetQuestByGUID(string QuestGUID) => ExtractionQuestSystem.GetQuestByGUID( QuestGUID );
}

public static class GuidGenerator
{
	public static string NewQuestId => "q-" + Guid.NewGuid().ToString();

	public static string NewObjectiveId => "o-" + Guid.NewGuid().ToString();

	public static string NewEntityId => "e-" + Guid.NewGuid().ToString();
}
