using Conna.Inventory;
using Sandbox;
using Sandbox.Services;
using static Sandbox.Services.Inventory;

public sealed class TempPlayerController : Component
{
	

	[Property] private bool IsHudEnable {  get; set; } = false;

	public TimeSince TimeAlive { get; set; } = 0;

	public PobxPlayerInventory Inventory { get; private set; }
	[Property] private PobxPlayerInventoryHud PlayerHud { get; set; }


	protected override void OnUpdate()
	{
		if ( Input.Pressed( "Inventory" ) )
		{
			ToggleInventoryHud();
		}
	}

	private void ToggleInventoryHud()
	{
		IsHudEnable = !IsHudEnable;
		PlayerHud.Enabled = IsHudEnable;
	}

	protected override void OnAwake()
	{
		// Create inventory with a unique ID (Component.Id works well)
		Inventory = new PobxPlayerInventory( Id, 5, 5 );
		Log.Info( Inventory );

		// Enable networking for multiplayer sync
		Inventory.Network.Enabled = true;
	}

	public InventoryResult AddItem(InventoryItem InventoryItem)
	{
		// Add to first available position (merges into existing stacks if possible)
		var result = Inventory.TryAdd( InventoryItem );
		Log.Info( result );

		// Add at specific position (no stack merging)
		result = Inventory.TryAddAt( InventoryItem, x: 2, y: 0 );

		return result;
	}
}
