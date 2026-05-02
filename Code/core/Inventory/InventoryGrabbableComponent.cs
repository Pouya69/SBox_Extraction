using Sandbox;

public sealed class InventoryGrabbableComponent : Component, IInteractable
{
	/// <summary>
	/// This will be used for dropping etc.
	/// </summary>
	[Property, Feature( "Inventory" )] protected PrefabScene ItemPrefab;
	[Property, Feature( "Inventory" )] protected Vector2Int ItemSizeInInventory = new(1,1);
	[Property, Feature( "Inventory" )] protected int InventoryMaxStackSize = 1;
	[Property, Feature( "Inventory" )] protected int CurrentCount = 1;
	/// <summary>
	/// If false, the item will be discarded upon dropping.
	/// </summary>
	[Property, Feature( "Inventory" )] public bool CanBeDropped { get; protected set; } = true;
	[Property] protected IExtractionQuestEntity EntityReference;
	protected PobxBaseInventoryItem pobxBaseInventoryItem;

	protected override void OnAwake()
	{
		pobxBaseInventoryItem = new PobxBaseInventoryItem( ItemPrefab, EntityReference.GetEntityName(), ItemSizeInInventory.x, ItemSizeInInventory.y, InventoryMaxStackSize, CurrentCount );
	}

	public void Interact( PlayerInteractionComponent interactionComponent )
	{
		var result = interactionComponent.AddItemToInventory( pobxBaseInventoryItem );

		if (result != Conna.Inventory.InventoryResult.Success)
		{
			return;
		}
		// Object is added. We can destroy the object now.

		this.GameObject.Destroy();


		//GameObject.Enabled = false;
		// Log.Info( pobxBaseInventoryItem.SpaceLeftInStack() );
		// Log.Info( $"{Rpc.Caller.DisplayName} interacted with {this.GameObject.Name}!" );
		//GameObject.Destroy();
	}

	public bool CanBePickedUp()
	{
		return false;
	}

	public bool IsPickUpTwoHanded()
	{
		return false;
	}

	/// <summary>
	/// Useful for when dropped, we set the prefab's count
	/// </summary>
	public void SetCount( int newCount ) {
		pobxBaseInventoryItem.StackCount = newCount;
		CurrentCount = newCount; 
	}
}
