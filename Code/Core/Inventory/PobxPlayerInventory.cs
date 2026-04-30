using Sandbox;
using Conna.Inventory;

public sealed class PobxPlayerInventory : BaseInventory
{
	public PobxPlayerInventory( Guid id ) : base( id, 5, 5 ) // 5 columns × 5 rows
	{

	}
}
