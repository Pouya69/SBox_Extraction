
using System;
using Sandbox.Citizen;
using Sandbox.Sboku.Shared;


public sealed class Weapon : Component, ISbokuWeapon
{
	public event Action<ISbokuWeapon> ReloadFinished;
	public event Action<ISbokuWeapon> NoAmmoLeft;

	[Property]
	private CitizenAnimationHelper.HoldTypes WeaponType { get; set; } = CitizenAnimationHelper.HoldTypes.None;
	
	[Property]
	private CitizenAnimationHelper.Hand WeaponHoldType { get; set; } = CitizenAnimationHelper.Hand.Both;

	public CitizenAnimationHelper.Hand GetWeaponHoldType() => WeaponHoldType;

	[Property] private GameObject MuzzleSocket;

	[Property] public float AttackCooldown { get; set; } = 0;
	[Property] private int MaxAmmoPerMagazine { get; set; } = 31;
	[Property] private int MaxMagazines { get; set; } = 4;
	[Property] private Collider WeaponCollider { get; set; }
	[Property] private Rigidbody WeaponRigidBody { get; set; }
	
	// [Property] private SkinnedModelRenderer WeaponRenderer;
	[Property] private ModelRenderer WeaponRenderer;
	
	private int Ammo { get; set; }
	private int MagazinesLeft { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Ammo =  MaxAmmoPerMagazine;
	}
	
	// public void PlayReloadAnimation() => WeaponRenderer.Set("b_reload", true);
	// public void PlayShootAnimation() => WeaponRenderer.Set("b_attack", true);

	public bool HasAmmo() => Ammo > 0;
	public bool HasMagazinesLeft() => MagazinesLeft > 0;

	public void ToggleWeaponPhysics(bool simulate)
	{
		WeaponRigidBody.MotionEnabled = simulate;
		WeaponRigidBody.Enabled = simulate;
		
		WeaponCollider.Enabled = simulate;
	}
	
	public CitizenAnimationHelper.HoldTypes GetWeaponType() => WeaponType;

	public void Shoot()
	{
		// global::Transform shootTransform = MuzzleSocket.WorldTransform;
		
		
	}
}
