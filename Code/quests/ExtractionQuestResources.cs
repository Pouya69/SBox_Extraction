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

	[ReadOnly]
	public string Quest_UID { get; private set; } = GuidGenerator.NewQuestId;

	public string Title { get; private set; }

	public string Description { get; private set; }

	public int XP_AfterCompletingQuest { get; private set; }

	public List<QuestObjectiveInfo> QuestObjectives { get; private set; }

	public List<ObjectiveRewardInfo> QuestCompletionRewards { get; private set; }

	public List<ObjectiveConditionInfo> FailureConditions { get; private set; }

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
	public bool ShouldShowToPlayer { get; private set; } = true;
	public bool IsOptional { get; private set; } = false;

	[ReadOnly]
	public string Objective_UID { get; private set; } = GuidGenerator.NewObjectiveId;

	public string Description { get; private set; }

	public int XP_AfterCompletingObjective { get; private set; }

	public List<ObjectiveRewardInfo> ObjectiveCompletionRewards { get; private set; }

	public ObjectiveConditionInfo SuccessCondition { get; private set; }
	public List<ObjectiveConditionInfo> FailureConditions { get; private set; }

	public List<QuestObjectiveInfo> LeadsTo { get; private set; }

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "error", width, height, "#ffffff", "black" );
	}
}

[AssetType( Name = "Location", Extension = "qloc", Category = "Quests" )]
public partial class QuestLocationInfo : GameResource
{
	[ReadOnly]
	public string Location_UID { get; private set; } = GuidGenerator.NewObjectiveId;

	public string LocationName { get; private set; }
	//public GameTags UID_LocationTag;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "home", width, height, "#fdea60", "black" );
	}
}

[AssetType( Name = "Entity", Extension = "qent", Category = "Quests" )]
public partial class QuestEntityInfo : GameResource
{
	/// <summary>
	/// When an entity is unique, only 1 can be existed in the entire game. e.g. an important NPC or item.
	/// </summary>
	public bool IsUnique { get; private set; } = false;

	[ReadOnly]
	public string Entity_UID { get; private set; } = GuidGenerator.NewEntityId;

	public string EntityName { get; private set; }

	//public GameTags UID_LocationTag;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "person", width, height, "#fdea60", "black" );
	}
}


[AssetType( Name = "Reward", Extension = "qreward", Category = "Quests" )]
public partial class ObjectiveRewardInfo : GameResource
{
	public GameObject ObjectRef { get; private set; }
	public int Amount { get; private set; }

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
	public bool WillFinishQuest { get; protected set; } = false;
	public EQuestObjectiveCondition Condition { get; protected set; }

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

	public int AmountNeeded { get; private set; } = 1;

	public QuestEntityInfo Entity { get; private set; }


	public override EQuestObjectiveResultType IsConditionMet( object CheckingObject, EQuestObjectiveCondition actionTaken, ExtractionPlayerQuestSystemHandlerComponent playerQuestSystem )
	{
		if ( actionTaken != Condition || CheckingObject != Entity ) return EQuestObjectiveResultType.NOT_RELAVANT;


		if ( playerQuestSystem.GetAmountInInventory(Entity) >= AmountNeeded)
			return EQuestObjectiveResultType.FULLY_DONE;

		return EQuestObjectiveResultType.PARTIALLY_DONE;
	}

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "person", width, height, "#fdea60", "black" );
	}
}

[AssetType( Name = "Location Condition", Extension = "qlcond", Category = "Quest Conditions" )]
public partial class ObjectiveLocationConditionInfo : ObjectiveConditionInfo
{
	public ObjectiveLocationConditionInfo()
	{
		Condition = EQuestObjectiveCondition.ENTERED;
	}

	public QuestLocationInfo Location { get; private set; }

	public override EQuestObjectiveResultType IsConditionMet( object CheckingObject, EQuestObjectiveCondition actionTaken, ExtractionPlayerQuestSystemHandlerComponent playerQuestSystem )
	{
		if ( actionTaken != Condition || CheckingObject != Location ) return EQuestObjectiveResultType.NOT_RELAVANT;

		return EQuestObjectiveResultType.FULLY_DONE;
	}

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "home", width, height, "#fdea60", "black" );
	}
}
