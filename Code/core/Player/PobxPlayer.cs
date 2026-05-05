using Conna.Inventory;

public sealed class PobxPlayer : Component
{
	private static PobxPlayer LocalPlayer { get; set; }
	public static PobxPlayer FindLocalPlayer() => LocalPlayer;
	public static T FindLocalWeapon<T>() where T : InventoryGrabbableComponent => FindLocalPlayer()?.GetComponentInChildren<T>( true );
	public bool IsLocalPlayer => !IsProxy;

	[Property, Feature( "Components" )] public GameObject Head { get; private set; }
	[Property, RequireComponent, Feature( "Components" )] public CharacterController Controller { get; private set; }
	[Property, RequireComponent, Feature( "Components" )] public PobxCharacterEntityComponent EntityComponent { get; private set; }
	[Property, RequireComponent, Feature( "Components" )] public SourceMovement SourceMovement { get; private set; }
	[Property, RequireComponent, Feature( "Components" )] public CameraComponent Camera { get; private set; }
	[Property, RequireComponent, Feature( "Components") ] private ActionSystemComponent ActionSystemComp { get; set; }
	[Property, RequireComponent, Feature( "Components" )] public PlayerInventoryComponent InventoryComponent { get; private set; }
	[Property, RequireComponent, Feature( "Components" )] private PobxPlayerInventoryHud InventoryHud { get; set; }
	[Property, RequireComponent, Feature( "Components" )] private PlayerInteractionComponent PlayerInteractionComponent { get; set; }

	public Transform EyeTransform
	{
		get
		{
			return Camera.WorldTransform;
		}
	}

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

		PlayerInteractionComponent?.OnControl();
		InventoryComponent?.OnControl();
		InventoryHud?.HandleInput();
	}
}
