using Conna.Inventory;
using Sandbox;

public sealed class PobxPlayer : Component
{
	public static PobxPlayer LocalPlayer;

	[Property, RequireComponent, Feature( "Components" )] public CharacterController Controller { get; private set; }
	[Property, RequireComponent, Feature( "Components" )] public SourceMovement SourceMovement { get; private set; }
	[Property, RequireComponent, Feature( "Components" )] public CameraComponent Camera { get; private set; }
	[Property, RequireComponent, Feature( "Components") ] private ActionSystemComponent ActionSystemComp { get; set; }
	[Property, RequireComponent, Feature( "Components" )] public PlayerInventoryComponent InventoryComponent { get; private set; }
	[Property, RequireComponent, Feature( "Components" )] private PobxPlayerInventoryHud InventoryHud { get; set; }
	[Property, RequireComponent, Feature( "Components" )] private PlayerInteractionComponent PlayerInteractionComponent { get; set; }


	// [Property] private PobxPlayerInventoryHud PlayerHud { get; set; }

	protected override void OnAwake()
	{
		
	}

	protected override void OnFixedUpdate()
	{
		OnControl();
	}

	private void OnControl()
	{
		/*
		if ( Input.UsingController )
		{
			SourceMovement.Enabled = !(Input.Down( "SpawnMenu" ) || Input.Down( "InspectMenu" ));
		}
		else
		{
			SourceMovement.Enabled = true;
		}
		*/

		PlayerInteractionComponent.OnControl();
		InventoryComponent.OnControl();
		InventoryHud?.HandleInput();
	}
}
