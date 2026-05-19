using Conna.Inventory;
using Sandbox;

public interface IInteractionComp
{
	public void AttemptInteract();
	public void Released();
	public GameObject GetGameObject();
	public GameObject GetAttachmentGameObject();

	public IExtractionQuestEntity GetEntity();

	InventoryResult AddItemToInventory( PobxBaseInventoryItem pobxBaseInventoryItem );
	void PickUpEntity( IExtractionQuestEntity entityRef );
}
