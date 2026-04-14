using Sandbox;

public sealed class WeaponPickUpOnly : Component
{
	[Property] private PrefabScene WeaponPrefab {get; set;}
	[Property] private Collider MyCollider {get; set;}

	protected override void OnStart()
	{
		MyCollider.OnObjectTriggerEnter += OnObjectTriggerEnter;
	}

	private void OnObjectTriggerEnter( GameObject objectEntered )
	{
		// if ( !objectEntered.Tags.Has( "player" ) ) return;

		var playerController = objectEntered.GetComponent<PlayerControllerExtension>();
		if ( playerController is null ) return;
		
		playerController.GiveWeapon( WeaponPrefab.Clone().GetComponent<Weapon>());
		DestroyGameObject();
	}
}
