using Sandbox;
using System.Security.AccessControl;

public sealed class ExtractionPlayerQuestSystemHandlerComponent : Component
{

	[ReadOnly, Property]
	public List<FTrackedQuest> ActiveQuests { get; private set; } = new();

	[ReadOnly, Property]
	public List<IExtractionQuest> CompletedQuests { get; private set; } = new();

	[ReadOnly, Property]
	public List<IExtractionQuest> FailedQuests { get; private set; } = new();

	[Property, Group("Debug")] private bool DebugQuestStats { get; set; }

	[Property, RequireComponent] private ExtractionQuestEntityComponent EntityComponent { get; set; }

	public void AddQuest( IExtractionQuest quest)
	{
		if ( DebugQuestStats )
		{
			string questStringDebug = quest.ToString();

			string logString = "*** NEW QUEST ADDED ***\n" + questStringDebug;
			Log.Info( logString );	
		}
		ActiveQuests.Add( new(quest) );
	}

	public void CompleteQuest( IExtractionQuest quest )
	{
		quest.QuestComplete();
		RemoveQuestFromActiveQuests( quest );
		CompletedQuests.Add(quest);
	}

	public void FailQuest( IExtractionQuest quest )
	{
		quest.QuestFailed();
		RemoveQuestFromActiveQuests( quest );
		FailedQuests.Add( quest );
	}

	/// <summary>
	/// Only call when transitioning between levels, or maybe death etc.
	/// </summary>
	public void UpdateAllQuests()
	{

	}

	public async void QuestObjectiveCompleted(ExtractionQuest quest, QuestObjectiveInfo objective)
	{
		await Task.Frame();

		RemoveObjectiveFromActiveObjectivesInQuest(quest, objective, true);
	}

	public async void QuestObjectiveFailed( ExtractionQuest quest, QuestObjectiveInfo objective )
	{
		await Task.Frame();

		RemoveObjectiveFromActiveObjectivesInQuest( quest, objective, false );
	}

	public void RemoveQuestFromActiveQuests( IExtractionQuest quest )
	{
		for ( int i = 0; i < ActiveQuests.Count; i++ )
		{
			var trackedQuest = ActiveQuests[i];

			if ( trackedQuest.Quest.GetQuest_GUID().Equals( quest.GetQuest_GUID() ) )
			{
				ActiveQuests.RemoveAt( i );
				return;
			}
		}
	}

	private void RemoveObjectiveFromActiveObjectivesInQuest(IExtractionQuest quest, QuestObjectiveInfo objective, bool wasQuestSuccessful = true) {
		for ( int i = 0; i < ActiveQuests.Count; i++ )
		{
			var trackedQuest = ActiveQuests[i];

			if ( trackedQuest.Quest.GetQuest_GUID().Equals( quest.GetQuest_GUID() ) )
			{
				for ( int j = 0; j < trackedQuest.ActiveObjectives.Count; j++ )
				{
					var activeObjective = trackedQuest.ActiveObjectives[i];

					if ( trackedQuest.ActiveObjectives[j].Objective_UID.Equals( objective.Objective_UID ) )
					{
						trackedQuest.ActiveObjectives.RemoveAt( j );

						if ( wasQuestSuccessful )
							trackedQuest.CompletedObjectives.Add( activeObjective );
						else
							trackedQuest.FailedObjectives.Add( activeObjective );

						return;
					}
				}

				return;
			}
		}
	}

	protected override void OnDisabled()
	{
		StopListeningToQuestSystemEvents();
	}

	private void Quests_OnLocationExited( IExtractionQuestEntity instigator, QuestLocationInfo obj )
	{
		// throw new System.NotImplementedException();
	}

	private void Quests_OnLocationEntered( IExtractionQuestEntity instigator, QuestLocationInfo location )
	{
		// Log.Info(instigator.GetEntityName() + " entered " + location.ToString() );

		foreach ( var quest in ActiveQuests )
		{
			var currentObjectives = quest.ActiveObjectives;

			// Will fail / complete / ignore the objective.
			foreach ( var currentObjective in currentObjectives )
			{
				if (DebugQuestStats)
					Log.Info( "Checking objective: " + currentObjective.Description );
				ExtractionQuestUtility.CheckQuestObjectiveConditions( quest.Quest, currentObjective, location, EQuestObjectiveCondition.ENTERED, this );
			}
			// We will not break it since there can be multiple quests that require the same stuff happening.
		}
	}

	private void Quests_OnEntityPickedUp( IExtractionQuestEntity instigator, IExtractionQuestEntity entity )
	{
		foreach ( var quest in ActiveQuests )
		{
			var currentObjectives = quest.ActiveObjectives;

			// Will fail / complete / ignore the objective.
			foreach ( var currentObjective in currentObjectives )
			{
				ExtractionQuestUtility.CheckQuestObjectiveConditions( quest.Quest, currentObjective, entity, EQuestObjectiveCondition.ENTERED, this );
			}
			// We will not break it since there can be multiple quests that require the same stuff happening.
		}
	}

	private void Quests_OnEntityKilled( IExtractionQuestEntity instigator, IExtractionQuestEntity entity )
	{
		foreach ( var quest in ActiveQuests )
		{
			var currentObjectives = quest.ActiveObjectives;

			// Will fail / complete / ignore the objective.
			foreach ( var currentObjective in currentObjectives )
			{
				ExtractionQuestUtility.CheckQuestObjectiveConditions( quest.Quest, currentObjective, entity, EQuestObjectiveCondition.ENTERED, this );
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

		ExtractionQuestSystem.Instance = new();

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

public struct FTrackedQuest
{
	public FTrackedQuest(IExtractionQuest quest)
	{
		Quest = quest;
		ActiveObjectives = quest.GetQuestStartingObjectives().ToList();
		FailedObjectives = new();
		CompletedObjectives = new();
	}

	public FTrackedQuest( IExtractionQuest quest, params QuestObjectiveInfo[] activeObjectives )
	{
		Quest = quest;
		ActiveObjectives = activeObjectives.ToList();
	}

	public IExtractionQuest Quest { get; private set; }
	public List<QuestObjectiveInfo> ActiveObjectives { get; private set; }
	public List<QuestObjectiveInfo> FailedObjectives { get; private set; }
	public List<QuestObjectiveInfo> CompletedObjectives { get; private set; }
}
