using Conna.Inventory;

public sealed class PlayerInventoryComponent : Component
{
	[Property, RequireComponent, Feature( "Components" )] private PobxPlayer Player { get; set; }
	[Property, RequireComponent, Feature( "Components" )] private PobxPlayerInventoryHud InventoryHud { get; set; }

	[Property, RequireComponent, Feature( "Inventory" ), Group( "Config" )] public PobxPlayerInventory Inventory { get; private set; }
	[Property, Feature( "Inventory" ), Group( "Config" )] private Vector2Int InventoryStorageSize = new( 3, 3 );
	[Property, Feature( "Inventory" ), Group("Config")] private InventorySlotMode InventorySlotMode = InventorySlotMode.Single;

	[Property, Feature( "Inventory" ), Group( "Drop" )] public float DropDistanceFromCameraForward { get; private set; } = 10.0f;
	[Property, Feature( "Inventory" ), Group( "Drop" )] private float DropScanRadius = 30.0f;

	public Weapon PrimaryWeapon { get; private set; }
	public Weapon SecondaryWeapon { get; private set; }

	public Weapon ActiveWeapon { get; private set; }
	public GadgetBase Gadget { get; private set; }

	public bool IsGadgetEquipped { get; private set; } = false;

	protected override void OnAwake()
	{
		Inventory = new PobxPlayerInventory( Id, InventoryStorageSize.x, InventoryStorageSize.y, InventorySlotMode );
		if (!InventoryHud.IsValid())
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

		if (Input.Pressed( "Slot1" ) )
		{
			SwitchToPrimaryWeapon();
		}
		else if ( Input.Pressed( "Slot2" ) )
		{
			SwitchToSecondaryWeapon();
		}
		else if ( Input.Pressed( "Slot3" ) )
			SwitchToSlot3OrMore( 3 );
		else if ( Input.Pressed( "Slot4" ) )
			SwitchToSlot3OrMore( 4 );
		else if ( Input.Pressed( "Slot5" ) )
			SwitchToSlot3OrMore( 5 );
		else if ( Input.Pressed( "Slot6" ) )
			SwitchToSlot3OrMore( 6 );
		else if ( Input.Pressed( "Slot7" ) )
			SwitchToSlot3OrMore( 7 );


		if ( IsGadgetEquipped )
			Gadget.OnControl( Player );
		else if ( ActiveWeapon.IsValid() )
			ActiveWeapon.OnControl( Player );

	}

	private void ToggleInventory()
	{
		// Log.Info( "Workin" );
		// Player.Controller.UseInputControls = false;
		InventoryHud.Enabled = !InventoryHud.Enabled;

	}

	public InventoryResult AddItemToInventory( PobxBaseInventoryItem item )
	{

		if (item.InventoryGrabbableReference.IsValid())
		{
			if ( (item.InventoryGrabbableReference as Weapon) is var weapon && weapon.IsValid())
			{
				if (!PrimaryWeapon.IsValid())
					PrimaryWeapon = weapon;
				else if (!SecondaryWeapon.IsValid())
					SecondaryWeapon = weapon;
				Log.Info( "Added Weapon" );
			}
			else if ( (item.InventoryGrabbableReference as GadgetBase) is var gadget && gadget.IsValid() )
			{
				if (!Gadget.IsValid())
				{
					Log.Info( "Gadget" );

					Gadget = gadget;
					Gadget.InitializeGadget( this.Player );
				}
				else
				{
					int OldCount =  Gadget.pobxBaseInventoryItem.StackCount;
					Gadget.pobxBaseInventoryItem.StackCount = Math.Clamp( Gadget.pobxBaseInventoryItem.StackCount + item.StackCount, 0, Gadget.pobxBaseInventoryItem.MaxStackSize );
					if ( OldCount != Gadget.pobxBaseInventoryItem.StackCount )
						gadget.DestroyGameObject();

					return InventoryResult.Success;
				}
			}

		}

		InventoryResult result = Inventory.TryAdd( item );
		if ( result == InventoryResult.ItemAlreadyInInventory )
		{
			if ( item.SpaceLeftInStack() > 0 )
			{
				item.StackCount = Math.Clamp( item.StackCount + item.StackCount, 0, item.MaxStackSize);
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
		Vector3 projectedSpawnPos = startPos + Player.EyeTransform.Forward * DropDistanceFromCameraForward;

		// We check so that if we hit something, we don't spawn behind the wall or smth.

		var traceResult = Scene.Trace.Sphere( DropScanRadius, startPos, projectedSpawnPos ).IgnoreGameObjectHierarchy( Player.GameObject ).UseHitPosition( true ).Run();

		if ( traceResult.Hit )
			projectedSpawnPos = traceResult.HitPosition;


		Rotation spawnRot = Rotation.Identity;

		if ( item.WillDestroyOnAdd )
		{
			GameObject spawnedObject = item.PobxItemReference.Clone( projectedSpawnPos, spawnRot );
			var grabComponent = spawnedObject.GetComponent<InventoryGrabbableComponent>();
			grabComponent.SetCount( item.StackCount );
			grabComponent.ItemRemovedFromInventory(this.Player);
		}
		else
		{
			item.InventoryGrabbableReference.GameObject.SetParent( null );
			item.InventoryGrabbableReference.WorldPosition = projectedSpawnPos;
			item.InventoryGrabbableReference.WorldRotation = spawnRot;
			item.InventoryGrabbableReference.ItemRemovedFromInventory(this.Player);
		}
		
		
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

	public void SwitchToPrimaryWeapon()
	{
		if (IsGadgetEquipped)
		{
			Gadget.DisableItem();
		}

		IsGadgetEquipped = false;

		if (ActiveWeapon.IsValid())
		{
			ActiveWeapon.DisableItem();
		}

		ActiveWeapon = PrimaryWeapon;
		ActiveWeapon.EnableItem();
	}

	public void SwitchToSecondaryWeapon()
	{
		if ( IsGadgetEquipped )
		{
			Gadget.DisableItem();
		}

		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.DisableItem();
		}

		IsGadgetEquipped = false;

		ActiveWeapon = SecondaryWeapon;
		ActiveWeapon.EnableItem();
	}

	/// <summary>
	/// Must be more than 2
	/// </summary>
	/// <param name="slotNumber"></param>
	public void SwitchToSlot3OrMore(int slotNumber)
	{
		if ( slotNumber <= 2 ) return;

		switch ( slotNumber ) {
			case 3:
				SwitchToGadget();
				break;
		}
	}

	public void DisableGadget()
	{
		IsGadgetEquipped = false;
		Gadget.DisableItem();
		Gadget = null;
	}

	private void SwitchToGadget()
	{
		if ( !Gadget.IsValid() ) return;

		if (!Gadget.CanBeEquipped() )
		{
			DisableGadget();
			return;
		}

		IsGadgetEquipped = true;
		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.DisableItem();
		}

		Gadget.EnableItem();
	}

}
