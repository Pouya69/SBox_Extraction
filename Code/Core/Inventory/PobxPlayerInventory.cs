using Sandbox;
using Conna.Inventory;

//public sealed class PobxPlayerInventory : BaseInventory
//{
//	public PobxPlayerInventory( Guid id ) : base( id, 5, 5 ) // 5 columns × 5 rows
//	{

//	}
//}

public class PobxPlayerInventory( Guid id, int width, int height, InventorySlotMode slotMode = InventorySlotMode.Single )
	: BaseInventory( id, width, height, slotMode );
