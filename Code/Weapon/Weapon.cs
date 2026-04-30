
using System;
using Sandbox.Citizen;
using Sandbox.Sboku.Shared;


public class Weapon : Component, ISbokuWeapon
{
	public event Action<ISbokuWeapon> ReloadFinished;
	public event Action<ISbokuWeapon> NoAmmoLeft;

	[Property]
	protected CitizenAnimationHelper.HoldTypes WeaponType { get; set; } = CitizenAnimationHelper.HoldTypes.None;
	
	[Property]
	protected CitizenAnimationHelper.Hand WeaponHoldType { get; set; } = CitizenAnimationHelper.Hand.Both;

	public CitizenAnimationHelper.Hand GetWeaponHoldType() => WeaponHoldType;

	[Property] protected GameObject MuzzleSocket;

	[Property] public float AttackCooldown { get; set; } = 0;
	[Property] protected int MaxAmmoPerMagazine { get; set; } = 31;
	[Property] protected int MaxMagazines { get; set; } = 4;
	[Property] protected Collider WeaponCollider { get; set; }
	[Property] protected Rigidbody WeaponRigidBody { get; set; }

	[Property] protected bool DebugWeaponActions { get; set; }

	public bool IsShooting { get; protected set; } = false;

	// [Property] protected SkinnedModelRenderer WeaponRenderer;
	[Property] protected ModelRenderer WeaponRenderer;
	
	protected int Ammo { get; set; }
	protected int MagazinesLeft { get; set; }

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

	public virtual void Shoot()
	{
		// global::Transform shootTransform = MuzzleSocket.WorldTransform;
		IsShooting = true;

	}

	public virtual void Reload()
	{

	}

	protected virtual void ShootWeapon()
	{

	}

	public virtual void StopShoot()
	{
		if ( !IsShooting ) return;

		IsShooting = false;
	}

	public virtual void ChangeFireMode()
	{

	}
}
