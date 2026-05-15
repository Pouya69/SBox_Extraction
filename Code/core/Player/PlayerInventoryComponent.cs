using Conna.Inventory;

public sealed class PlayerInventoryComponent : Component, Global.IPlayerEvents
{
	[Property, RequireComponent, Feature( "Components" )] private PobxPlayer Player { get; set; }
	public PobxPlayerInventoryHud InventoryHud => Player?.InventoryHud;

	[Property, Feature( "Inventory" ), Group( "Config" )] public PobxPlayerInventory Inventory => Player?.PlayerState?.PlayerInventory;
	[Property, Feature( "Inventory" ), Group( "Config" )] private Vector2Int InventoryStorageSize = new( 3, 3 );
	[Property, Feature( "Inventory" ), Group("Config")] private InventorySlotMode InventorySlotMode = InventorySlotMode.Single;

	[Property, Feature( "Inventory" ), Group( "Drop" )] public float DropDistanceFromCameraForward { get; private set; } = 10.0f;
	[Property, Feature( "Inventory" ), Group( "Drop" )] private float DropScanRadius = 30.0f;


	public VacuumGun VacuumGun => Inventory.VacuumGun;
	public EnergyPistolWeapon PistolWeapon => Inventory.PistolWeapon;


	public Weapon ActiveWeapon { get; set; }
	public GadgetBase Gadget => Inventory.Gadget;

	public bool IsGadgetEquipped { get; private set; } = false;

	protected override void OnStart()
	{
		
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
			SwitchToSlot( 1 );
		}
		else if ( Input.Pressed( "Slot2" ) )
		{
			SwitchToSlot( 2 );
		}
		else if ( Input.Pressed( "Slot3" ) )
			SwitchToSlot( 3 );
		else if ( Input.Pressed( "Slot4" ) )
			SwitchToSlot( 4 );
		else if ( Input.Pressed( "Slot5" ) )
			SwitchToSlot( 5 );
		else if ( Input.Pressed( "Slot6" ) )
			SwitchToSlot( 6 );
		else if ( Input.Pressed( "Slot7" ) )
			SwitchToSlot( 7 );


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
				if ( (weapon as EnergyPistolWeapon) is var pistol && pistol.IsValid() )
				{
					if ( !PistolWeapon.IsValid() )
						Inventory.PistolWeapon = pistol;
				}
				else if ( (weapon as VacuumGun) is var vacuum && vacuum.IsValid() )
				{
					if ( !VacuumGun.IsValid() )
						Inventory.VacuumGun = vacuum;
				}
				Log.Info( "Added Weapon" );
			}
			else if ( (item.InventoryGrabbableReference as GadgetBase) is var gadget && gadget.IsValid() )
			{
				if (!Gadget.IsValid())
				{
					Log.Info( "Gadget" );

					Inventory.Gadget = gadget;
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

		if ( !Player.IsValid() )
		{
			return;
		}

		Vector3 startPos = Player.Head.WorldPosition;
		Vector3 projectedSpawnPos = startPos + Player.EyeTransform.Forward * DropDistanceFromCameraForward;

		// DebugOverlay.Line(new Line( startPos, projectedSpawnPos ), Color.Red, 9.0f);


		// We check so that if we hit something, we don't spawn behind the wall or smth.

		var traceResult = Scene.Trace.Sphere( DropScanRadius, startPos, projectedSpawnPos ).IgnoreGameObjectHierarchy( Player.GameObject ).UseHitPosition( true ).Run();

		if ( traceResult.Hit )
		{
			// DebugOverlay.Sphere( new Sphere( traceResult.HitPosition, 10.0f ), Color.Red, 9.0f );
			projectedSpawnPos = traceResult.HitPosition;
		}


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
			item.InventoryGrabbableReference.WorldPosition = projectedSpawnPos;
			item.InventoryGrabbableReference.WorldRotation = spawnRot;

			item.InventoryGrabbableReference.GameObject.SetParent( null );
			item.InventoryGrabbableReference.ItemRemovedFromInventory(this.Player);

			item.InventoryGrabbableReference.WorldPosition = projectedSpawnPos;
			item.InventoryGrabbableReference.WorldRotation = spawnRot;


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

	/// <summary>
	/// Must be more than 2
	/// </summary>
	/// <param name="slotNumber"></param>
	public void SwitchToSlot(int slotNumber)
	{
		switch ( slotNumber ) {
			case 1:
				SwitchToWeapon();
				ActiveWeapon = PistolWeapon;
				ActiveWeapon?.EnableItem();
				break;

			case 2:
				SwitchToWeapon();
				ActiveWeapon = VacuumGun;
				ActiveWeapon?.EnableItem();
				break;

			case 5:
				SwitchToGadget();
				break;
		}
	}

	private void SwitchToWeapon()
	{
		if ( IsGadgetEquipped )
		{
			Gadget.DisableItem();
		}

		IsGadgetEquipped = false;

		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.DisableItem();
		}
	}

	public void DisableGadget()
	{
		IsGadgetEquipped = false;
		Gadget.DisableItem();
		Inventory.Gadget = null;
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

	void Global.IPlayerEvents.OnPlayerSpawned()
	{
		if ( Player?.PlayerState?.PlayerInventory == null)
			Player?.PlayerState?.PlayerInventory = new PobxPlayerInventory( Id, InventoryStorageSize.x, InventoryStorageSize.y, InventorySlotMode );
		else
		{
			
		}

		InventoryHud.PlayerInitialized();
		SubscribeToInventoryEvents();
	}

	void Global.IPlayerEvents.OnPlayerDied()
	{
		ActiveWeapon?.DisableItem();
	}

}
