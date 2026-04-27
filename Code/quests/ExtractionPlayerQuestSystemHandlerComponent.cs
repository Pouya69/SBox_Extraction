using Sandbox;

public sealed class ExtractionPlayerQuestSystemHandlerComponent : Component
{

	protected override void OnEnabled()
	{
		UpdateAllQuests();
	}

	[ReadOnly, Property]
	public List<IExtractionQuest> ActiveQuests {  get; private set; }

	[ReadOnly, Property]
	public List<IExtractionQuest> CompletedQuests { get; private set; }

	[ReadOnly, Property]
	public List<IExtractionQuest> FailedQuests { get; private set; }

	public void AddQuest( IExtractionQuest quest)
	{
		ActiveQuests.Add(quest);
	}

	public void CompleteQuest( IExtractionQuest quest )
	{
		quest.QuestComplete();
		ActiveQuests.Remove(quest);
		CompletedQuests.Add(quest);
	}

	public void FailQuest( IExtractionQuest quest )
	{
		quest.QuestFailed();
		ActiveQuests.Remove( quest );
		FailedQuests.Add( quest );
	}

	/// <summary>
	/// Only call when transitioning between levels, or maybe death etc.
	/// </summary>
	public void UpdateAllQuests()
	{

	}

	public IExtractionQuest GetQuestByGUID( string QuestGUID ) => ExtractionQuestSystem.GetQuestByGUID( QuestGUID );
}
