using Conna.Inventory;

public sealed class PlayerInventoryComponent : Component
{
	[Property, RequireComponent, Feature( "Components" )] private PobxPlayer Player { get; set; }
	[Property, RequireComponent, Feature( "Components" )] private PobxPlayerInventoryHud InventoryHud { get; set; }

	[Property, RequireComponent, Feature( "Inventory" ), Group( "Config" )] public PobxPlayerInventory Inventory { get; private set; }
	[Property, Feature( "Inventory" ), Group( "Config" )] private Vector2Int InventoryStorageSize = new( 3, 3 );
	[Property, Feature( "Inventory" ), Group("Config")] private InventorySlotMode InventorySlotMode = InventorySlotMode.Single;

	[Property, Feature( "Inventory" ), Group( "Drop" )] private float DropDistanceFromCameraForward = 10.0f;
	[Property, Feature( "Inventory" ), Group( "Drop" )] private float DropScanRadius = 30.0f;

	private Weapon ActiveWeapon;

	protected override void OnAwake()
	{
		Inventory = new PobxPlayerInventory( Id, InventoryStorageSize.x, InventoryStorageSize.y, InventorySlotMode );
		InventoryHud = Scene.Get<PobxPlayerInventoryHud>();

		SubscribeToInventoryEvents();
		
	}

	protected override void OnDestroy()
	{
		UnsubscribeToInventoryEvents();
	}

	public void OnControl()
	{
		if (Input.Pressed("Inventory"))
		{
			ToggleInventory();
		}

		if ( ActiveWeapon.IsValid() )
			ActiveWeapon.OnPlayerUpdate(Player);
	}

	private void ToggleInventory()
	{
		// Log.Info( "Workin" );
		// Player.Controller.UseInputControls = false;
		InventoryHud.Enabled = !InventoryHud.Enabled;

	}

	public InventoryResult AddItemToInventory( PobxBaseInventoryItem item )
	{
		InventoryResult result = Inventory.TryAdd( item );
		if ( result == InventoryResult.ItemAlreadyInInventory )
		{
			if ( item.SpaceLeftInStack() > 0 )
			{
				item.StackCount += item.StackCount;
				result = InventoryResult.Success;
			}
		}

		return result;
	}

	private void OnItemRemovedFromInventory( PobxBaseInventoryItem item )
	{

		if (!item.PobxItemReference.GetComponent<InventoryGrabbableComponent>().CanBeDropped)
		{
			// Item will be discarded.
			return;
		}

		Vector3 startPos = Player.Camera.WorldPosition;
		Vector3 projectedSpawnPos = startPos + Player.Camera.WorldTransform.Forward * DropDistanceFromCameraForward;

		// We check so that if we hit something, we don't spawn behind the wall or smth.

		var traceResult = Scene.Trace.Sphere( DropScanRadius, startPos, projectedSpawnPos ).IgnoreGameObjectHierarchy( Player.GameObject ).UseHitPosition( true ).Run();

		if ( traceResult.Hit )
			projectedSpawnPos = traceResult.HitPosition;


		Rotation spawnRot = Rotation.Identity;

		GameObject spawnedObject = item.PobxItemReference.Clone(projectedSpawnPos, spawnRot);
		var grabComponent = spawnedObject.GetComponent<InventoryGrabbableComponent>();
		grabComponent.SetCount( item.StackCount );
	}

	private void OnItemAddedToInventory( BaseInventory.Entry entry )
	{
		var item = entry.Item as PobxBaseInventoryItem;
	}

	private void SubscribeToInventoryEvents()
	{
		Inventory.OnItemFullyDroppedFromInventory += OnItemRemovedFromInventory;
		Inventory.OnItemAdded += OnItemAddedToInventory;
	}

	private void UnsubscribeToInventoryEvents()
	{
		Inventory.OnItemFullyDroppedFromInventory -= OnItemRemovedFromInventory;
		Inventory.OnItemAdded -= OnItemAddedToInventory;
	}
}
