using Sandbox;
using Conna.Inventory;
using SimpleInteractions;

public sealed class BallItem : PobxBaseInventoryItem
{
	public override string DisplayName => "Ball Item";
	public override int Width => 1;
	public override int Height => 1;
	public override int MaxStackSize => 1;
}

public class PobxBaseInventoryItem : InventoryItem
{
	public SimpleInteraction PobxItemReference;
}
