using Sandbox;
using Sandbox.UI;
using System.Text.Json.Serialization;

public enum EQuestObjectiveCondition
{
	KILLED,
	NOT_KILLED,
	ENTERED,
	SPOKEN_TO,
	GRABBED,
	NOT_GRABBED,
	GIVEN_TO,
	NOT_GIVEN_TO
}

public enum EQuestObjectiveResultType
{
	NOT_RELAVANT,
	FULLY_DONE,
	PARTIALLY_DONE,
}

[AssetType( Name = "Quest", Extension = "quest", Category = "Quests" )]
public partial class QuestInfo : GameResource
{
	//public GameTags Quest_UID {get; private set;}

	[ReadOnly] [JsonInclude]
	public string Quest_UID { get; private set; } = GuidGenerator.NewQuestId;

	[JsonInclude]
	public string Title { get; private set; }

	[JsonInclude]
	public string Description { get; private set; }

	/// <summary>
	/// For when the user is given multiple objectives at the start. e.g. an optional AND a real one.
	/// </summary>

	[JsonInclude]
	public int XP_AfterCompletingQuest { get; private set; }

	[JsonInclude]
	public List<QuestObjectiveInfo> QuestObjectives { get; private set; }

	[JsonInclude]
	public List<ObjectiveRewardInfo> QuestCompletionRewards { get; private set; }

	[JsonInclude]
	public List<ObjectiveConditionInfo> FailureConditions { get; private set; }

	[JsonInclude]
	public List<QuestObjectiveInfo> StartingObjectives { get; protected set; }

	[JsonInclude]
	public List<QuestInfo> QuestCompletionLeadsTo { get; protected set; }

	[JsonInclude]
	public List<QuestInfo> QuestFailureLeadsTo { get; protected set; }

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "mobile", width, height, "#fdea60", "red" );
	}
}

[AssetType( Name = "Quest Generic Objective", Extension = "qobj", Category = "Quests" )]
public partial class QuestObjectiveInfo : GameResource
{

	/// <summary>
	/// Should the player know how many are left to kill? Or hidden for in general (e.g. 4/10 Grab amount of items. Or 2/5 Kill enmies.)
	/// It could also be used for hiding the objective from the player (no UI).
	/// </summary>
	[JsonInclude] public bool ShouldShowToPlayer { get; private set; } = true;
	[JsonInclude] public bool IsOptional { get; private set; } = false;

	[ReadOnly] [JsonInclude]
	public string Objective_UID { get; private set; } = GuidGenerator.NewObjectiveId;

	[JsonInclude] public string Description { get; private set; }

	[JsonInclude] public int XP_AfterCompletingObjective { get; private set; }

	[JsonInclude] public List<ObjectiveRewardInfo> ObjectiveCompletionRewards { get; private set; }


	/// <summary>
	/// WORKS ONLY AS "OR" OPERATOR. IF YOU WANT "AND", ADD A NEW OBJECTIVE.
	/// </summary>
	[JsonInclude] public List<ObjectiveConditionInfo> SuccessConditions { get; private set; }
	[JsonInclude] public List<ObjectiveConditionInfo> FailureConditions { get; private set; }

	[JsonInclude] public List<QuestObjectiveInfo> LeadsTo { get; private set; }

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "error", width, height, "#ffffff", "black" );
	}
}

[AssetType( Name = "Location", Extension = "qloc", Category = "Quests" )]
public partial class QuestLocationInfo : GameResource
{

	[ReadOnly] [JsonInclude]
	public string Location_UID { get; private set; } = GuidGenerator.NewLocationId;

	[JsonInclude] public string LocationName { get; private set; }
	//public GameTags UID_LocationTag;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "home", width, height, "#fdea60", "black" );
	}

	public override string ToString()
	{
		return LocationName;
	}
}

[AssetType( Name = "Entity", Extension = "qent", Category = "Quests" )]
public partial class QuestEntityInfo : GameResource
{

	/// <summary>
	/// When an entity is unique, only 1 can be existed in the entire game. e.g. an important NPC or item.
	/// </summary>
	[JsonInclude] public bool IsUnique { get; private set; } = false;

	[ReadOnly] [JsonInclude]
	public string Entity_UID { get; private set; } = GuidGenerator.NewEntityId;

	[JsonInclude] public string EntityName { get; private set; }

	//public GameTags UID_LocationTag;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "person", width, height, "#fdea60", "black" );
	}
}


[AssetType( Name = "Reward", Extension = "qreward", Category = "Quests" )]
public partial class ObjectiveRewardInfo : GameResource
{
	[JsonInclude] public GameObject ObjectRef { get; private set; }
	[JsonInclude] public int Amount { get; private set; }

	//public GameTags UID_LocationTag;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "trophy", width, height, "#fdea60", "black" );
	}
}

public partial class ObjectiveConditionInfo : GameResource
{
	/// <summary>
	/// Sometimes we want it to not fail/scuess the quest. But fail/success an objective only.
	/// </summary>
	[JsonInclude] public bool WillFinishQuest { get; protected set; } = false;
	[JsonInclude] public EQuestObjectiveCondition Condition { get; protected set; }

	public virtual EQuestObjectiveResultType IsConditionMet( object CheckingObject, EQuestObjectiveCondition actionTaken, ExtractionPlayerQuestSystemHandlerComponent playerQuestSystem ) => EQuestObjectiveResultType.FULLY_DONE;

	//public GameTags UID_LocationTag;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "trophy", width, height, "#fdea60", "black" );
	}
}

[AssetType( Name = "Entity Condition", Extension = "qecond", Category = "Quest Conditions" )]
public partial class ObjectiveEntityConditionInfo : ObjectiveConditionInfo
{
	public ObjectiveEntityConditionInfo()
	{
		Condition = EQuestObjectiveCondition.GRABBED;
	}

	[JsonInclude] public int AmountNeeded { get; private set; } = 1;

	[JsonInclude] public QuestEntityInfo Entity { get; private set; }

	/// <summary>
	/// Used for when a condition needs 2 entities. Leave empty if condition relies on 1 entity.
	/// e.g. artifact GIVE_TO Pouya. artifact is Entity, Pouya is Entity_02.
	/// </summary>
	[JsonInclude] public QuestEntityInfo Entity_02 { get; private set; }


	public override EQuestObjectiveResultType IsConditionMet( object CheckingObject, EQuestObjectiveCondition actionTaken, ExtractionPlayerQuestSystemHandlerComponent playerQuestSystem )
	{
		if ( actionTaken != Condition || CheckingObject != Entity ) return EQuestObjectiveResultType.NOT_RELAVANT;


		switch (actionTaken)
		{
			case EQuestObjectiveCondition.GRABBED:
				if ( playerQuestSystem.GetAmountInInventory( Entity ) >= AmountNeeded )
					return EQuestObjectiveResultType.FULLY_DONE;

				return EQuestObjectiveResultType.PARTIALLY_DONE;

			default:
				if ( playerQuestSystem.GetAmountInInventory( Entity ) >= AmountNeeded )
					return EQuestObjectiveResultType.FULLY_DONE;

				return EQuestObjectiveResultType.PARTIALLY_DONE;
		}

		
	}

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "person", width, height, "#fdea60", "black" );
	}

	public override string ToString()
	{
		return "Condition: " + Condition + ", on entity: " + Entity.EntityName + ((Entity_02 == null) ? "" : ", entity 2: " + Entity_02.EntityName);
	}
}

[AssetType( Name = "Location Condition", Extension = "qlcond", Category = "Quest Conditions" )]
public partial class ObjectiveLocationConditionInfo : ObjectiveConditionInfo
{
	public ObjectiveLocationConditionInfo()
	{
		Condition = EQuestObjectiveCondition.ENTERED;
	}

	[JsonInclude] public QuestLocationInfo Location { get; private set; }

	public override EQuestObjectiveResultType IsConditionMet( object CheckingObject, EQuestObjectiveCondition actionTaken, ExtractionPlayerQuestSystemHandlerComponent playerQuestSystem )
	{
		if ( actionTaken != Condition || CheckingObject != Location ) return EQuestObjectiveResultType.NOT_RELAVANT;

		return EQuestObjectiveResultType.FULLY_DONE;
	}

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "home", width, height, "#fdea60", "black" );
	}

	public override string ToString()
	{
		return "Condition: " + Condition + ", on location: " + Location.LocationName;
	}
}

