using Sandbox;

public sealed class ExtractionPlayerQuestSystemHandlerComponent : Component
{

	[ReadOnly, Property]
	public List<IExtractionQuest> ActiveQuests {  get; private set; }

	[ReadOnly, Property]
	public List<IExtractionQuest> CompletedQuests { get; private set; }

	[ReadOnly, Property]
	public List<IExtractionQuest> FailedQuests { get; private set; }

	public void AddQuest( IExtractionQuest quest)
	{
		ActiveQuests.Add( quest );
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


	protected override void OnDisabled()
	{
		StopListeningToQuestSystemEvents();
	}

	private void Quests_OnLocationExited( QuestLocationInfo obj )
	{
		// throw new System.NotImplementedException();
	}

	private void Quests_OnLocationEntered( QuestLocationInfo location )
	{
		foreach ( var quest in ActiveQuests )
		{
			var currentObjectives = quest.GetCurrentObjectives();

			// Will fail / complete / ignore the objective.
			foreach ( var currentObjective in currentObjectives )
			{
				ExtractionQuestUtility.CheckQuestObjectiveConditions( quest, currentObjective.ObjectiveInfo, location, EQuestObjectiveCondition.ENTERED, this );
			}
			// We will not break it since there can be multiple quests that require the same stuff happening.
		}
	}

	private void Quests_OnEntityPickedUp( IExtractionQuestEntity entity )
	{
		foreach ( var quest in ActiveQuests )
		{
			var currentObjectives = quest.GetCurrentObjectives();

			// Will fail / complete / ignore the objective.
			foreach ( var currentObjective in currentObjectives )
			{
				ExtractionQuestUtility.CheckQuestObjectiveConditions( quest, currentObjective.ObjectiveInfo, entity, EQuestObjectiveCondition.ENTERED, this );
			}
			// We will not break it since there can be multiple quests that require the same stuff happening.
		}
	}

	private void Quests_OnEntityKilled( IExtractionQuestEntity entity )
	{
		foreach ( var quest in ActiveQuests )
		{
			var currentObjectives = quest.GetCurrentObjectives();

			// Will fail / complete / ignore the objective.
			foreach ( var currentObjective in currentObjectives )
			{
				ExtractionQuestUtility.CheckQuestObjectiveConditions( quest, currentObjective.ObjectiveInfo, entity, EQuestObjectiveCondition.ENTERED, this );
			}

			
			
			// We will not break it since there can be multiple quests that require the same stuff happening.
		}
	}

	public int GetAmountInInventory(QuestEntityInfo entity) {
		throw new System.NotImplementedException();
	}


	public IExtractionQuest GetQuestByGUID( string QuestGUID ) => ExtractionQuestSystem.GetQuestByGUID( QuestGUID );


	protected override void OnEnabled()
	{
		UpdateAllQuests();

		StartListeningToQuestSystemEvents();
	}

	private void StartListeningToQuestSystemEvents()
	{
		ExtractionQuestSystem.Instance.OnEntityKilled += Quests_OnEntityKilled;
		ExtractionQuestSystem.Instance.OnEntityPickedUp += Quests_OnEntityPickedUp;
		ExtractionQuestSystem.Instance.OnLocationEntered += Quests_OnLocationEntered;
		ExtractionQuestSystem.Instance.OnLocationExited += Quests_OnLocationExited;
	}


	private void StopListeningToQuestSystemEvents()
	{
		ExtractionQuestSystem.Instance.OnEntityKilled -= Quests_OnEntityKilled;
		ExtractionQuestSystem.Instance.OnEntityPickedUp -= Quests_OnEntityPickedUp;
		ExtractionQuestSystem.Instance.OnLocationEntered -= Quests_OnLocationEntered;
		ExtractionQuestSystem.Instance.OnLocationExited -= Quests_OnLocationExited;
	}
}
