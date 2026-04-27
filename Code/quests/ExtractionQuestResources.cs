using Sandbox;


[AssetType( Name = "Quest", Extension = "quest", Category = "Quests" )]
public partial class QuestInfo : GameResource
{
	//public GameTags Quest_UID {get; set;}

	[ReadOnly]
	public string Quest_UID { get; private set; } = GuidGenerator.NewQuestId;

	public string Title { get; set; }

	public string Description { get; set; }

	public int XP_AfterCompletingQuest { get; set; }

	public List<QuestObjectiveInfo> QuestObjectives { get; set; }

	public List<ObjectiveRewardInfo> QuestCompletionRewards { get; set; }

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
	public bool ShouldShowToPlayer { get; set; } = true;
	public bool IsOptional { get; set; } = false;

	[ReadOnly]
	public string Quest_UID { get; private set; } = GuidGenerator.NewObjectiveId;

	public string Description { get; set; }

	public int XP_AfterCompletingObjective { get; set; }

	public List<ObjectiveRewardInfo> ObjectiveCompletionRewards { get; set; }

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "mobile", width, height, "#ffffff", "black" );
	}
}

[AssetType( Name = "Quest Fetch Objective", Extension = "fqobj", Category = "Quests" )]
public partial class QuestFetchObjectiveInfo : QuestObjectiveInfo
{
	//public GameTags UID_ObjectTag;

	public GameObject ObjectReference { get; set; }

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "error", width, height, "#fdea60", "black" );
	}
}

[AssetType( Name = "Quest Multiple Fetch Objective", Extension = "mfqobj", Category = "Quests" )]
public partial class QuestMultipleFetchObjectiveInfo : QuestFetchObjectiveInfo
{

	public int Amount { get; set; }

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "error", width, height, "#fdea60", "black" );
	}
}

[AssetType( Name = "Quest Kill Objective", Extension = "mkqobj", Category = "Quests" )]
public partial class QuestKillObjectiveInfo : QuestObjectiveInfo
{
	//public GameTags UID_NPCTag { get; set; }

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "bomb", width, height, "#fdea60", "black" );
	}
}

[AssetType( Name = "Quest Multiple Kill Objective", Extension = "kqobj", Category = "Quests" )]
public partial class QuestMultipleKillObjectiveInfo : QuestKillObjectiveInfo
{
	public int Amount {  get; set; }

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "swords", width, height, "#ffffff", "black" );
	}
}

[AssetType( Name = "Quest Location Objective", Extension = "lqobj", Category = "Quests" )]
public partial class QuestLocationObjectiveInfo : QuestObjectiveInfo
{
	public string LocationName {  get; set; }
	//public GameTags UID_LocationTag;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "location_on", width, height, "#fdea60", "black" );
	}
}

[AssetType( Name = "Location", Extension = "qloc", Category = "Quests" )]
public partial class QuestLocationInfo : GameResource
{
	[ReadOnly]
	public string Quest_UID { get; private set; } = GuidGenerator.NewObjectiveId;

	public string LocationName { get; set; }
	//public GameTags UID_LocationTag;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "home", width, height, "#fdea60", "black" );
	}
}

[AssetType( Name = "Reward", Extension = "qreward", Category = "Quests" )]
public partial class ObjectiveRewardInfo : GameResource
{
	public GameObject ObjectRef { get; set; }
	public int Amount { get; set; }

	//public GameTags UID_LocationTag;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
	{
		return CreateSimpleAssetTypeIcon( "trophy", width, height, "#fdea60", "black" );
	}
}
