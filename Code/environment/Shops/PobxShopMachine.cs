using Sandbox;

public sealed class PobxShopMachine : ContainerBase, IShopInterface, IInteractable
{
	/// <summary>
	/// Actual shop. This items in ContainerItems are a pool to spawn things from.
	/// Randomizes based on FContainerItem.Amount (x, y)
	/// </summary>
	[Property, Feature("Shop"), ReadOnly] private List<ShopItem> _shopItems { get; set; } = new();
	[Property, Feature( "Shop" ), RequireComponent] private Pobx_MedShop_HudComponent Shop_Hud_Component { get; set; }

	[Property, Feature( "Shop" )] private int _shopCredits { get; set; }

	public PobxPlayerState CurrentPlayerUsingThisShop { get; protected set; }

	public event Action OnShopChanged;

	public bool IsShopInUse() => CurrentPlayerUsingThisShop != null;

	protected override void OnStart()
	{
		// base.OnStart();
		RefreshShopItems(true);
	}

	public void RefreshShopItems( bool firstTime = false )
	{
		if (!firstTime)
			_shopItems.Clear();

		foreach ( var item in ContainerItems )
		{
			ShopItem shopItem = new(item.Cost, item.ObjectPrefab,
				(item.AmountMultiplier > 0 ? item.AmountMultiplier : 1) * Random.Shared.Next( item.Amount.x, item.Amount.y ) );

			_shopItems.Add( shopItem );
		}

		OnShopChanged?.Invoke();
	}


	public bool RemoveItemFromShop(IShopItem item, int amount)
	{
		var itemIndex = GetItemRefInShop( item.GetObjectPrefab(), out var outShopItemRef );
		if ( itemIndex < 0 )
			return false;

		outShopItemRef.Amount -= amount;
		if ( outShopItemRef.Amount <= 0 )
		{
			_shopItems.RemoveAt( itemIndex );
			RemoveFromContainerShop( item.GetObjectPrefab() );
			Log.Info( $"Removing item {item.GetObjectPrefab().Name} from shop {this.GameObject.Name}" );
		}
		else
			_shopItems[itemIndex] = outShopItemRef;

		return true;
	}

	public int GetCountInShop( IShopItem item ) => GetItemRefInShop( item.GetObjectPrefab(), out var outShopItemRef ) == -1 ? 0 : outShopItemRef.Amount;

	public int GetItemRefInShop(PrefabScene prefab, out ShopItem outItem) {
		outItem = null;

		for ( int i = _shopItems.Count - 1; i > -1; i-- )
		{
			var shopItem = _shopItems[i];
			if ( !shopItem.GetObjectPrefab().Equals( prefab ) )
				continue;

			outItem = shopItem;
			return i;
		}

		return -1;
	}

	public bool SellItem( InventoryGrabbableComponent itemToSell, int amount, PobxPlayerState playerState )
	{
		var itemIndex = GetItemRefInShop( itemToSell.ItemWorldModelPrefab, out var outShopItemRef );
		if ( itemIndex < 0 )
		{
			// Item doesn't exist we should create a new one.
			outShopItemRef = new( itemToSell.Cost, itemToSell.ItemWorldModelPrefab, itemToSell.pobxBaseInventoryItem.StackCount );
		}

		int amountToSell = Math.Min( amount, itemToSell.pobxBaseInventoryItem.StackCount );


		_shopItems.Add( outShopItemRef );

		if ( amountToSell >= itemToSell.pobxBaseInventoryItem.StackCount )
		{
			// Remove from player inventory completely
			var result = playerState.PlayerInventory.TryRemove( itemToSell.pobxBaseInventoryItem );
			if ( result != Conna.Inventory.InventoryResult.Success )
			{
				Log.Error($"{playerState.DisplayName} tried to sell. But item did not exist in inventory?! result: {result}");
				return false;
			}
		}
		else
		{
			// Reduce the amount.
			itemToSell.pobxBaseInventoryItem.StackCount -= amountToSell;
		}

		outShopItemRef.SoldToShop(this, playerState);

		int creditsWorth = amountToSell * outShopItemRef.GetCost();
		int creditsToAdd = Math.Min( _shopCredits, creditsWorth );
		_shopCredits = Math.Max( _shopCredits - creditsToAdd, 0);

		playerState.AddCredits( creditsToAdd );

		OnShopChanged?.Invoke();

		Log.Info( $"{playerState.DisplayName} sold {amountToSell} {itemToSell.GameObject.Name} from {this.GameObject.Name}, worth {creditsWorth}. And got {creditsToAdd}. New Shop Credits: {_shopCredits}" );

		return true;
	}

	public bool BuyItem( IShopItem itemToBuy, int amount, PobxPlayerState playerState )
	{
		int amountToBuy = Math.Min( amount, GetCountInShop( itemToBuy ) );

		int creditsNeeded = itemToBuy.GetCost() * amountToBuy;

		if ( !playerState.HasEnoughCredits( creditsNeeded ) || !playerState.CurrentPlayerReference.IsValid() )
		{
			Log.Info( $"{playerState.DisplayName} does not have enough credits. Needed: {creditsNeeded}, They have {playerState.Credits}" );
			return false;
		}

		var hadEnoughCredits = playerState.TakeCredits( creditsNeeded );
		Assert.True( hadEnoughCredits );

		_shopCredits += creditsNeeded;
		itemToBuy.BoughtByPlayer( this, playerState );

		var itemSpawned = itemToBuy.GetObjectPrefab().Clone();

		if (itemSpawned == null)
		{
			Log.Error( $"Shop {this.GameObject.Name} tried to spawn an item but it didn't work... Check the items." );
			return false;
		}

		bool removedFromShop = RemoveItemFromShop( itemToBuy, amountToBuy );
		Assert.True( removedFromShop );

		if ( itemSpawned.GetComponent<InventoryGrabbableComponent>( true ) is var grabbableComp && grabbableComp != null )
		{
			// Inventory Item

			grabbableComp.Cost = itemToBuy.GetCost();

			Invoke( 0.05f, () =>
			{
				playerState.CurrentPlayerReference.InventoryComponent.AddItemToInventory( grabbableComp.pobxBaseInventoryItem );
			} );
		}
		else if ( itemSpawned.GetComponent<ConsumableBase>( true ) is var consumable && consumable != null )
		{
			// Ammo etc.
			consumable.Cost = itemToBuy.GetCost();
			consumable.Interact( playerState.CurrentPlayerReference.PlayerInteractionComponent );
		}
		else
		{
			Log.Error($"{playerState.DisplayName} tried to buy but Item Typed not recognized for {itemSpawned.Name}");
			return false;
		}

		
		


		Log.Info( $"{playerState.DisplayName} bought {amountToBuy} {itemToBuy.GetObjectPrefab().Name} from {this.GameObject.Name}, worth {creditsNeeded}. New Shop Credits: {_shopCredits}" );
		OnShopChanged?.Invoke();

		return true;
	}



	public bool BuyItem( int indexOfitemToBuy, int amount, PobxPlayerState playerState )
	{
		if ( indexOfitemToBuy < 0 || indexOfitemToBuy >= ShopTotalItemCount() ) return false;
		return BuyItem( _shopItems[indexOfitemToBuy], amount, playerState );
	}

	[Group( "Debug" ), Button( "Give 200 Credits To Player" )]
	public void Debug_Give200Credits()
	{
		PobxPlayer.FindLocalPlayer().PlayerState.AddCredits( 200 );
	}

	[Group( "Debug" ), Button( "Give 10 Credits To Player" )]
	public void Debug_Give10Credits()
	{
		PobxPlayer.FindLocalPlayer().PlayerState.AddCredits( 10 );
	}

	[Group("Debug"), Button("Debug Buy 10 items")]
	public void Debug_TestBuy()
	{
		BuyItem( 0, 10, PobxPlayer.FindLocalPlayer().PlayerState );
	}

	[Group( "Debug" ), Button( "Debug Sell 10 items" )]
	public void Debug_TestSell()
	{
		SellItem( (PobxPlayer.FindLocalPlayer().PlayerState.PlayerInventory.GetItemAt(0,0) as PobxBaseInventoryItem)?.InventoryGrabbableReference , 10, PobxPlayer.FindLocalPlayer().PlayerState );
	}



	public bool RemoveFromContainerShop(PrefabScene objectPrefab) {
		var itemFound = ContainerItems.Find( x => x.ObjectPrefab.Equals( objectPrefab ) );
		if ( itemFound.ObjectPrefab == null || !itemFound.IsUnique )
			return false;

		return ContainerItems.Remove( itemFound );
	}

	public ContainerBase GetContainer() => this;
	public GameObject GetGameObject() => GameObject;

	public IEnumerable<ShopItem> GetShopItems() => _shopItems;

	public int GetShopCredits() => _shopCredits;

	public void Interact( IInteractionComp interactionComponent )
	{
		var playerInteractionComp = interactionComponent as PlayerInteractionComponent;

		if ( !playerInteractionComp.IsValid() )
			return;

		OpenShop( playerInteractionComp.Player.PlayerState );
	}

	public bool CanBePickedUp() => false;

	public bool IsPickUpTwoHanded() => false;

	public bool IsInteractable() => IsShopInUse();

	public bool OpenShop( PobxPlayerState playerState )
	{
		if ( !IsInteractable() )
			return false;

		CurrentPlayerUsingThisShop = playerState;

		if ( CurrentPlayerUsingThisShop.IsMe )
			Shop_Hud_Component.Enabled = true;

		return true;
	}

	public void CloseShop()
	{
		if ( CurrentPlayerUsingThisShop.IsMe )
			Shop_Hud_Component.Enabled = false;

		CurrentPlayerUsingThisShop = null;
	}

	public int ShopTotalItemCount() => _shopItems.Count;

	public PobxPlayerState GetCurrentPlayerUsingShop() => CurrentPlayerUsingThisShop;
}
