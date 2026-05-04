using Conna.Inventory;
using Sandbox;

public sealed class PobxPlayer : Component, IExtractionQuestEntity
{
	public static PobxPlayer LocalPlayer;

	[Property, RequireComponent, Feature( "Components" )] public CharacterController Controller { get; private set; }
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

	public void AddEntityToGlobalManager()
	{
		throw new NotImplementedException();
	}

	public bool IsAlive()
	{
		throw new NotImplementedException();
	}

	public void EnteredArea( QuestLocationInfo location )
	{
		throw new NotImplementedException();
	}

	public void EntityKilled( IExtractionQuestEntity Instigator )
	{
		throw new NotImplementedException();
	}

	public void EntityPickedUp( IExtractionQuestEntity Instigator )
	{
		throw new NotImplementedException();
	}

	public string GetEntityName()
	{
		throw new NotImplementedException();
	}

	public EExtractionObjectSize GetObjectSize()
	{
		throw new NotImplementedException();
	}

	public bool CanBeRemoteGrabbed()
	{
		throw new NotImplementedException();
	}

	public Renderer GetRenderer()
	{
		throw new NotImplementedException();
	}

	public void ToggleEnablePhysics( bool enable )
	{
		throw new NotImplementedException();
	}

	public void LaunchEntity( Vector3 velocity, bool ignoreMass = true )
	{
		throw new NotImplementedException();
	}

	public GameObject GetGameObject()
	{
		throw new NotImplementedException();
	}

	public Rigidbody GetRigidbody()
	{
		throw new NotImplementedException();
	}

	public Collider GetCollider()
	{
		throw new NotImplementedException();
	}
}
