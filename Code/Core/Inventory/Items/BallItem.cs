using Sandbox;
using Conna.Inventory;

public sealed class BallItem : InventoryItem
{
	public override string DisplayName => "Ball Item";
	public override int Width => 1;
	public override int Height => 1;
	public override int MaxStackSize => 1;
}
