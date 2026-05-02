using Sandbox;
using Conna.Inventory;

//public sealed class PobxPlayerInventory : BaseInventory
//{
//	public PobxPlayerInventory( Guid id ) : base( id, 5, 5 ) // 5 columns × 5 rows
//	{

//	}
//}

public class PobxPlayerInventory( Guid id, int width, int height, InventorySlotMode slotMode = InventorySlotMode.Single ) : BaseInventory( id, width, height, slotMode )
{
	/// <summary>
	/// When it is fully dropped. OnItemRemoved is used for other actions as wel. This one is drop and remove only.
	/// </summary>
	public Action<PobxBaseInventoryItem> OnItemFullyDroppedFromInventory;

	public InventoryResult RemoveItemAndDropFromInventory(PobxBaseInventoryItem item)
	{
		var result = TryRemove( item );

		if (result == InventoryResult.Success)
			OnItemFullyDroppedFromInventory?.Invoke(item);

		return result;
	}
}
