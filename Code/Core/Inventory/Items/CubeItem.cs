using Sandbox;
using Conna.Inventory;

public class CubeItem : InventoryItem
{
	public override string DisplayName => "Cube Item";
	public override int Width => 1;
	public override int Height => 1;
	public override int MaxStackSize => 1;
}
