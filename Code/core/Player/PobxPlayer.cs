using Conna.Inventory;

public sealed class PobxPlayer : Component
{
	public PobxPlayerState PlayerState { get; set;  }

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
	public PobxPlayerInventoryHud InventoryHud { get; private set; }
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
		if (IsLocalPlayer)
		{
			LocalPlayer = this;
			InventoryHud = Scene.GetComponentInChildren<PobxPlayerInventoryHud>(true);
		}

		ActionSystemComp.OnDamaged += OnDamaged;
		ActionSystemComp.OnDeath += OnDeath;
	}

	protected override void OnDestroy()
	{
		this.ActionSystemComp.OnDamaged -= OnDamaged;
	}

	private void OnDamaged( GameObject arg1, GameObject arg2, float arg3, float arg4 )
	{
		
	}

	private void OnDeath( GameObject obj )
	{
		Global.IPlayerEvents.Post(x => x.OnPlayerDied());

		this.InventoryComponent.ActiveWeapon?.DestroyViewModel();
		this.InventoryComponent.Gadget?.GameObject.Enabled = false;
		this.SourceMovement.DisableInput();

		GameManager.Current.RespawnPlayer( this.PlayerState );

		// this.Enabled = false;
	}

	protected override void OnFixedUpdate()
	{
		if ( !this.SourceMovement.IsInputEnabled ) return;

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
