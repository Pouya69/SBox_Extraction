using Sandbox;

public class InventoryGrabbableComponent : Component, IInteractable
{
	/// <summary>
	/// This will be used for dropping etc.
	/// </summary>
	[Property, Feature( "Inventory" )] protected PrefabScene ItemWorldModelPrefab;
	[Property, Feature( "Inventory" )] protected Renderer Renderer;
	[Property, Feature( "Inventory" )] protected Vector2Int ItemSizeInInventory = new(1,1);
	[Property, Feature( "Inventory" )] protected int InventoryMaxStackSize = 1;
	[Property, Feature( "Inventory" )] protected int CurrentCount = 1;
	public GameObject WorldModel { get; protected set; }
	/// <summary>
	/// If false, the item will be discarded upon dropping.
	/// </summary>
	[Property, Feature( "Inventory" )] public bool CanBeDropped { get; protected set; } = true;
	[Property] protected IExtractionQuestEntity EntityReference;
	protected PobxBaseInventoryItem pobxBaseInventoryItem;

	protected override void OnAwake()
	{
		WorldModel = Renderer.GameObject;
		pobxBaseInventoryItem = new PobxBaseInventoryItem( ItemWorldModelPrefab, EntityReference.GetEntityName(), ItemSizeInInventory.x, ItemSizeInInventory.y, InventoryMaxStackSize, CurrentCount, WillBeDestroyedOnAddToInventory(), this );
	}

	protected override void OnEnabled()
	{
		// EnableItem();
	}

	protected override void OnDisabled()
	{
		// DisableItem();
	}

	public virtual bool WillBeDestroyedOnAddToInventory() => true;

	public void Interact( PlayerInteractionComponent interactionComponent )
	{
		var result = interactionComponent.AddItemToInventory( pobxBaseInventoryItem );

		if (result != Conna.Inventory.InventoryResult.Success)
		{
			return;
		}
		// Object is added. We can destroy the object now.

		AddedItemToInventory( interactionComponent );


		//GameObject.Enabled = false;
		// Log.Info( pobxBaseInventoryItem.SpaceLeftInStack() );
		// Log.Info( $"{Rpc.Caller.DisplayName} interacted with {this.GameObject.Name}!" );
		//GameObject.Destroy();
	}

	protected virtual void AddedItemToInventory( PlayerInteractionComponent interactionComponent )
	{
		ExtractionQuestSystem.EntityPickedUp( interactionComponent.Player, EntityReference );
		if (pobxBaseInventoryItem.WillDestroyOnAdd)
			this.GameObject.Destroy();
	}

	public virtual void ItemRemovedFromInventory() {
		DisableItem();
	}

	public virtual void EnableItem() { }
	public virtual void DisableItem() { }

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

	/// <summary>
	/// Should be implemented in the derived classes.
	/// </summary>
	public virtual void OnControl( PobxPlayer Player ) { }
}
