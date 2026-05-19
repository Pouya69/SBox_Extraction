using Sandbox;

public sealed class PobxShopMachine : ContainerBase, IShopInterface, IInteractable
{
	/// <summary>
	/// Actual shop. This items in ContainerItems are a pool to spawn things from.
	/// Randomizes based on FContainerItem.Amount (x, y)
	/// </summary>
	[Property, Feature("Shop"), ReadOnly] private List<ShopItem> _shopItems { get; set; } = new();
	[Property, Feature( "Shop" )] private int _shopCredits { get; set; }

	public PobxPlayerState CurrentPlayerUsingThisShop { get; protected set; }
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
		}
		else
			_shopItems[itemIndex] = outShopItemRef;

		return true;
	}

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


		_shopItems.Add( outShopItemRef );

		if ( amount >= itemToSell.pobxBaseInventoryItem.StackCount )
		{
			// Remove from player inventory completely
			var result = playerState.PlayerInventory.TryRemove( itemToSell.pobxBaseInventoryItem );
			if ( result != Conna.Inventory.InventoryResult.Success )
			{
				Log.Error($"{playerState.DisplayName} tried to sell. But item did not exist in inventory?!");
				return false;
			}
		}
		else
		{
			// Reduce the amount.
			itemToSell.pobxBaseInventoryItem.StackCount -= amount;
		}

		outShopItemRef.SoldToShop(this, playerState);

		int creditsWorth = amount * outShopItemRef.GetCost();
		int creditsToAdd = Math.Min( _shopCredits, amount * outShopItemRef.GetCost());
		_shopCredits = Math.Max( creditsToAdd, 0);

		playerState.AddCredits( creditsToAdd );

		Log.Info( $"{playerState.DisplayName} sold {amount} {itemToSell.GameObject.Name} from {this.GameObject.Name}, worth {creditsWorth}. And got {creditsToAdd}. New Shop Credits: {_shopCredits}" );

		return true;
	}

	public bool BuyItem( IShopItem itemToBuy, int amount, PobxPlayerState playerState )
	{
		int creditsNeeded = itemToBuy.GetCost() * amount;

		if ( !playerState.HasEnoughCredits( creditsNeeded ) || !playerState.CurrentPlayerReference.IsValid() )
		{
			Log.Info( $"{playerState.DisplayName} does not have enough credits. Needed: {creditsNeeded}, They have {playerState.Credits}" );
			return false;
		}

		var hadEnoughCredits = playerState.TakeCredits( creditsNeeded );
		Assert.True( hadEnoughCredits );

		_shopCredits += creditsNeeded;
		itemToBuy.BoughtByPlayer( this, playerState );

		var inventoryGrabbableSpawned = itemToBuy.GetObjectPrefab().Clone();

		if (inventoryGrabbableSpawned == null)
		{
			Log.Error( $"Shop {this.GameObject.Name} tried to spawn an item but it didn't work... Check the items." );
			return false;
		}

		var grabbableComp = inventoryGrabbableSpawned.GetComponent<InventoryGrabbableComponent>();

		bool removedFromShop = RemoveItemFromShop( itemToBuy, amount);
		Assert.True( removedFromShop );
		
		playerState.CurrentPlayerReference.InventoryComponent.AddItemToInventory( grabbableComp.pobxBaseInventoryItem );

		Log.Info( $"{playerState.DisplayName} bought {amount} {itemToBuy.GetObjectPrefab().Name} from {this.GameObject.Name}, worth {creditsNeeded}. New Shop Credits: {_shopCredits}" );

		return true;
	}



	public bool BuyItem( int indexOfitemToBuy, int amount, PobxPlayerState playerState )
	{
		if ( indexOfitemToBuy < 0 || indexOfitemToBuy >= _shopItems.Count ) return false;
		return BuyItem( _shopItems[indexOfitemToBuy], amount, playerState );
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
		return true;
	}

	public void CloseShop()
	{
		CurrentPlayerUsingThisShop = null;
	}
}
