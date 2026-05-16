using Sandbox;

public enum EAmmoType
{
	ENERGY,
	SHOTGUN,
	RIFLE
}

public sealed class AmmoPickable : ConsumableBase
{


	[Property, Feature( "Ammo" )] public int Ammo { get; private set; } = 5;
	[Property, Feature( "Ammo" )] public EAmmoType AmmoType { get; private set; }

	public override void Interact( PlayerInteractionComponent interactionComponent )
	{

		if ( !interactionComponent.Player.InventoryComponent.GiveAmmo( AmmoType, Ammo ) ) return;

		DestroyGameObject();

	}

	public override void Consume()
	{
		
	}
}
