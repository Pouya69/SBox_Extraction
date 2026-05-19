using Sandbox;

public interface IShopInterface
{
	public GameObject GetGameObject();
	public ContainerBase GetContainer();
	public bool BuyItem( IShopItem itemToBuy, int amount, PobxPlayerState playerState);
	public bool BuyItem( int indexOfitemToBuy, int amount, PobxPlayerState playerState);

	public bool SellItem( InventoryGrabbableComponent itemToSell, int amount, PobxPlayerState playerState );
	public void RefreshShopItems(bool firstTime = false);
	public IEnumerable<ShopItem> GetShopItems();
	public int GetShopCredits();

	public bool OpenShop(PobxPlayerState playerState);

	public PobxPlayerState GetCurrentPlayerUsingShop();

	public void CloseShop();
	public int ShopTotalItemCount();

	public event Action OnShopChanged;
}

public interface IShopItem
{
	public PrefabScene GetObjectPrefab();
	public int GetCost();
	public void SoldToShop( IShopInterface shopSoldTo, PobxPlayerState playerThatSold );
	public void BoughtByPlayer( IShopInterface shopBoughtFrom, PobxPlayerState playerThatBought );
}

public class ShopItem : IShopItem
{
	private int _cost;
	private PrefabScene _objectPrefab;
	public int Amount { get; set; }

	public ShopItem(int cost, PrefabScene objectPrefab, int amount)
	{
		_objectPrefab= objectPrefab;
		_cost = cost;
		Amount = amount;
	}

	public void BoughtByPlayer( IShopInterface shopBoughtFrom, PobxPlayerState playerThatBought )
	{
		
	}

	public void SoldToShop( IShopInterface shopSoldTo, PobxPlayerState playerThatSold )
	{
		
	}
	public int GetCost() => _cost;
	public PrefabScene GetObjectPrefab() => _objectPrefab;

}

public struct FShopItem
{
	public FShopItem()
	{
	}

	public int Amount { get; set; }
	public PrefabScene ObjectPrefab { get; set; }
	public int Cost { get; set; }
}
