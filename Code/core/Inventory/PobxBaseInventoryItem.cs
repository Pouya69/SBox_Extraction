using Sandbox;
using Conna.Inventory;
using SimpleInteractions;

public class PobxBaseInventoryItem : InventoryItem
{
	public PrefabScene PobxItemReference;
	public string displayName;
	public int width;
	public int height;
	public int maxStackSize;
	public int startingStackCount;

	public PobxBaseInventoryItem( PrefabScene pobxItemReference, string displayName, int width, int height, int maxStackSize, int startingStackCount ) {
		PobxItemReference = pobxItemReference;
		this.displayName = displayName;
		this.width = width;
		this.height = height;
		this.maxStackSize = maxStackSize;

		this.startingStackCount = startingStackCount;
		this.StackCount = startingStackCount;
	}

	public override string DisplayName => displayName;
	public override int Width => width;
	public override int Height => height;
	public override int MaxStackSize => maxStackSize;

	public override InventoryItem CreateStackClone( int stackCount )
	{
		var description = TypeLibrary.GetType( GetType() );
		var clone = description.Create<PobxBaseInventoryItem>( [PobxItemReference, displayName, width, height, maxStackSize, startingStackCount] );
		clone.StackCount = stackCount;

		return clone;
	}
}
