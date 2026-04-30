using Sandbox;

public sealed class TempPlayerController : Component
{
	[Property] public float Health { get; set; } = 100f;
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float Armor { get; set; } = 0f;
	[Property] public float MaxArmor { get; set; } = 100f;
	[Property] public int Coins { get; set; } = 0;

	public TimeSince TimeAlive { get; set; } = 0;

	public PobxPlayerInventory Inventory { get; private set; }

	protected override void OnAwake()
	{
		// Create inventory with a unique ID (Component.Id works well)
		Inventory = new PobxPlayerInventory( Id );

		// Enable networking for multiplayer sync
		Inventory.Network.Enabled = true;
	}
}
