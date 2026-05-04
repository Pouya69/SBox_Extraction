
using Sandbox.Citizen;
using Sandbox.Sboku.Shared;
using System;
using System.Numerics;
using static Sandbox.VideoWriter;


public class Weapon : InventoryGrabbableComponent, ISbokuWeapon
{
	public event Action<Weapon> OnMagazineIn;
	public event Action<Weapon> OnMagazineOut;
	public event Action<Weapon> OnReloadFinished;
	public event Action<Weapon> OnNoAmmoLeft;

	[Property, Feature( "Weapon" ), Group( "References" )] protected BulletPoolingComponent BulletPoolingComp { get; set; }
	[Property, Feature( "Weapon" ), Group( "References" )] protected PrefabScene BulletPrefab { get; set; }
	[Property, Feature( "Weapon" ), Group( "References" )] protected PrefabScene WeaponViewModelPrefab;
	public GameObject ViewModel { get; protected set; }

	[Property, Feature( "Weapon" ), Group( "References" )] protected GameObject MuzzleSocket;
	[Property, Feature( "Weapon" ), Group( "References" ), RequireComponent] protected Collider WeaponCollider { get; set; }
	[Property, Feature( "Weapon" ), Group( "References" ), RequireComponent] protected Rigidbody WeaponRigidBody { get; set; }

	[Property, Feature( "Weapon" ), Group( "Config" )]
	public bool HasSecondaryAttack { get; protected set; } = false;

	/// <summary>
	/// For weapons that have 'infinite ammo' or things that don't reload.
	/// Used for input checks.
	/// </summary>
	[Property, Feature( "Weapon" ), Group( "Config" )]
	protected bool IsReloadable { get; set; } = true;

	[Property, Feature( "Weapon" ), Group( "Config" )]
	protected bool ShouldReloadAutomatically{ get; set; } = true;

	/// <summary>
	/// For weapons that have 'infinite ammo' or things that don't reload.
	/// Used for input checks.
	/// </summary>
	[Property, Feature( "Weapon" ), Group( "Config" )]
	protected bool UsesBullets { get; set; } = true;


	[Property, Feature( "Weapon" ), Group( "Config" )]
	protected CitizenAnimationHelper.HoldTypes WeaponType { get; set; } = CitizenAnimationHelper.HoldTypes.None;
	
	[Property, Feature( "Weapon" ), Group( "Config" )]
	protected CitizenAnimationHelper.Hand WeaponHoldType { get; set; } = CitizenAnimationHelper.Hand.Both;

	/// <summary>
	/// Lowers the amount of recoil / visual noise when aiming
	/// </summary>
	[Property, Feature( "Weapon" ), Group( "Config" )] public float IronSightsFireScale { get; set; } = 0.2f;

	[Property, Feature( "Weapon" ), Group( "Config" )] protected bool DebugWeaponActions { get; set; }


	[Property, Feature( "Weapon" ), Group( "Weapon" )] public float AttackCooldown { get; set; } = 0;

	/// <summary>
	/// If FireRate <= 0, the gun will be considered as single shot (e.g. Pistols)
	/// Time between each shot.
	/// </summary>
	[Property, Feature( "Weapon" ), Group( "Weapon" )] public float FireRate { get; protected set; } = 0.0f;
	[Property, Feature( "Weapon" ), Group( "Weapon" )] protected int MaxAmmoPerMagazine { get; set; } = 31;
	[Property, Feature( "Weapon" ), Group( "Weapon" )] protected int MaxBullets { get; set; } = 300;
	[Property, Feature( "Weapon" ), Group( "Weapon" )] protected int BulletsShotPerAttack { get; set; } = 1;
	// [Property, Feature( "Weapon" ), Group( "Weapon" )] protected int BulletsShotPerAttack { get; set; } = 1;
	[Property, Feature( "Weapon" ), Group( "Weapon" )] public int ReserveBulletsLeft { get; protected set; }



	[Property, Feature( "Shooting Config" )] public BulletConfiguration BulletConfig { get; protected set; } = new() {
		Damage = 12f,
		BulletRadius = 1f,
		Range = 4096f,
		AimConeBase = new Vector2( 0.5f, 0.25f ),
		AimConeSpread = new Vector2( 3f, 3f ),
		AimConeRecovery = 0.2f,
		RecoilPitch = new Vector2( -0.3f, -0.1f ),
		RecoilYaw = new Vector2( -0.1f, 0.1f ),
		CameraRecoilStrength = 1f,
		CameraRecoilFrequency = 1f,
	};

	/// <summary>
	/// The bullet max scan range for tracing.
	/// </summary>

	protected TimeSince CurrentFireTime;

	public CitizenAnimationHelper.Hand GetWeaponHoldType() => WeaponHoldType;

	/// <summary>
	/// If we don't use weapon pooling, we will spawn the bullet every shot.
	/// </summary>
	/// <returns></returns>
	public bool IsUsingBulletPooling() => BulletPoolingComp.IsValid();

	public bool IsSingleShotWeapon => FireRate <= 0.0f;

	public bool IsShooting { get; protected set; } = false;
	public bool IsReloading { get; protected set;  } = false;
	public bool IsAiming { get; protected set; } = false;

	public int Ammo { get; protected set; }

	/// <summary>
	/// The owner of this carriable
	/// </summary>
	public PobxPlayer Owner
	{
		get
		{
			return GetComponentInParent<PobxPlayer>( true );
		}
	}

	public bool HasOwner => Owner.IsValid();

	public Ray AimRay
	{
		get
		{
			if ( HasOwner )
			{
				var owner = Owner;
				if ( owner.Controller.IsValid() && Scene.Camera.IsValid() )
					return Scene.Camera.Transform.World.ForwardRay;

				return owner.EyeTransform.ForwardRay;
			}

			/*
			var seated = ClientInput.Current;
			if ( seated.IsValid() && IsTargetedAim && Scene.Camera.IsValid() )
				return Scene.Camera.Transform.World.ForwardRay;
			*/

			var muzzle = MuzzleSocket.WorldTransform;
			return new Ray( muzzle.Position, muzzle.Rotation.Forward );
		}
	}


	protected override void OnAwake()
	{
		base.OnAwake();
		Ammo =  MaxAmmoPerMagazine;
		WorldModel = Renderer.GameObject;
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		CurrentFireTime = 0.0f;
	}
	
	// public void PlayReloadAnimation() => WeaponRenderer.Set("b_reload", true);
	// public void PlayShootAnimation() => WeaponRenderer.Set("b_attack", true);

	public bool HasAmmo() => Ammo > 0;
	public bool HasReserveBulletsLeft() => ReserveBulletsLeft > 0;

	public void ToggleWeaponPhysics(bool simulate)
	{
		WeaponRigidBody.MotionEnabled = simulate;
		WeaponRigidBody.Enabled = simulate;
		
		WeaponCollider.Enabled = simulate;
	}
	
	public CitizenAnimationHelper.HoldTypes GetWeaponType() => WeaponType;

	public virtual void Reload()
	{
		IsReloading = true;
		ViewModel?.RunEvent<ViewModel>( x => x.OnReloadStart() );
	}

	public virtual void ReloadFinished()
	{
		IsReloading = false;
		OnReloadFinished?.Invoke( this );
		ViewModel?.RunEvent<ViewModel>( x => x.OnReleoadFinish() );
	}

	/// <summary>
	/// The real shooting happens here.
	/// </summary>
	protected virtual void ShootWeapon()
	{
		int AmountsToShoot = TakeAmmo( BulletsShotPerAttack );
		if (AmountsToShoot == 0)
		{
			OutOfAmmo();
			return;
		}

		CurrentFireTime = 0.0f;
		ViewModel?.RunEvent<ViewModel>( x => x.OnAttack() );
		// @TODO: add check for NOT bullet pooling.
		if ( IsUsingBulletPooling() )
		{
			bool DidGrabBulletFromPool = BulletPoolingComp.PopBulletFromPool( out PobxBullet outBulletFromPool );
			if (!DidGrabBulletFromPool)
			{
				Log.Warning( "Bullet pool not big enough for fire rate and bullet properties for " + this.GameObject.Name +
					". Increase the bullet pool size for this weapon. Or Change the fire rate etc." );
				SpawnBulletAndShoot();
				return;
			}
			// Normal shooting that was grabbed from bullet pool.
			InitializeBulletAndShoot( outBulletFromPool );

			return;
		}

		// When not using bullet pooling.
		SpawnBulletAndShoot();
	}

	public virtual bool AddBulletToPool(PobxBullet bullet)
	{
		if ( !IsUsingBulletPooling() ) return false;
		BulletPoolingComp.AddBulletToPool( bullet );
		return true;
	}

	protected virtual void OutOfAmmo()
	{
		if ( IsReloading ) return;

		if (IsReloadable && ShouldReloadAutomatically && ReserveBulletsLeft > 0)
		{
			Reload();
			Log.Info( "Out ammo reloading" );
		}

		Log.Info( "Out of ammo" );

		IsShooting = false;
		// @TODO: Sounds etc.
		OnNoAmmoLeft?.Invoke( this );
	}

	private void SpawnBulletAndShoot()
	{
		PobxBullet spawnedBullet = BulletPoolingComponent.SpawnBullet( BulletPrefab, this );
		InitializeBulletAndShoot( spawnedBullet );
	}

	protected virtual void InitializeBulletAndShoot( PobxBullet bullet )
	{
		var viewModel = ViewModel.GetComponent<WeaponModel>();
		var bulletTransform = viewModel.GetTracerOrigin();

		var bulletPos = bulletTransform.Position;
		Vector3 bulletForward = bulletTransform.Forward;
		Ray aimRay = AimRay;
		
		// var traceEndPos = traceStartPos + ();
		var traceResult = Scene.Trace.Ray(aimRay, BulletConfig.Range).IgnoreGameObjectHierarchy(this.GameObject.Parent).WithTag( "projectile" ).Radius( BulletConfig.BulletRadius ).UseHitPosition().UseHitboxes().Run();
		if (traceResult.Hit)
		{
			bulletForward = (traceResult.HitPosition - bulletPos).Normal;
		}

		var bulletRot = Rotation.LookAt( bulletForward );

		bullet.InitializeBullet( bulletPos, bulletRot, BulletConfig );
	}

	public virtual void ChangeFireMode()
	{

	}

	/// <summary>
	/// By default we have PrimaryAttack, SecondaryAttack, and Reload.
	/// For other controls you can override this. (ChangeFireMode etc.)
	/// </summary>
	/// <param name="Player"></param>
	public override void OnControl( PobxPlayer Player )
	{
		// base.OnControl( Player );
		if ( CanReload() )
		{
			if (Input.Pressed("Reload"))
			{
				Reload();
			}
		}

		if ( Input.Pressed( "Attack1" ) )
		{
			if ( CanPrimaryAttack() )
				PrimaryAttack();
			else if ( !HasAmmo() )
				OutOfAmmo();
		}
		else if ( Input.Released( "Attack1" ) )
		{
			StopPrimaryAttack();
		}

		if ( IsShooting && CanPrimaryAttack() && (!IsSingleShotWeapon && CurrentFireTime >= FireRate) )
		{
			ShootWeapon();
		}

		if ( Input.Pressed( "Attack2" ) )
		{
			if ( HasSecondaryAttack && CanSecondaryAttack() )
				SecondaryAttack();
			else
				Aim();
		}
		else if ( Input.Released( "Attack2" ) )
		{
			if ( HasSecondaryAttack )
				SecondaryAttack();
			else
				StopAim();
		}


	}

	protected override void OnUpdate()
	{
		
	}

	protected virtual void PrimaryAttack()
	{
		IsShooting = true;
		ShootWeapon();
	}

	protected virtual void SecondaryAttack() {
		IsShooting = true;
	}

	protected virtual bool CanPrimaryAttack() => HasAmmo() && !IsReloading;
	protected virtual bool CanSecondaryAttack() => false;



	public virtual void StopPrimaryAttack()
	{
		if ( !IsShooting ) return;

		IsShooting = false;

		ViewModel?.RunEvent<ViewModel>( x => x.OnStopAttack() );
	}

	public virtual void StopSecondaryAttack()
	{
		if ( !IsShooting ) return;

		IsShooting = false;
	}

	/// <summary>
	/// For specific behaviours.
	/// </summary>
	/// <returns></returns>
	protected virtual bool CanReload() { return IsReloadable; }

	protected void CreateViewModel()
	{
		if ( ViewModel.IsValid() )
			return;

		DestroyViewModel();

		if ( WeaponViewModelPrefab == null )
			return;

		ViewModel = WeaponViewModelPrefab.Clone(new CloneConfig { Parent = GameObject, StartEnabled = false, Transform = global::Transform.Zero} );
		ViewModel.Flags |= GameObjectFlags.NotSaved | GameObjectFlags.NotNetworked | GameObjectFlags.Absolute;
		ViewModel.Enabled = true;
		ViewModel.Tags.Add( "firstperson", "viewmodel" );

		var viewModelComp = ViewModel.GetComponent<ViewModel>();
		viewModelComp.Controller = GetComponentInParent<SourceMovement>( false, false );

		if ( viewModelComp.IsValid() )
			viewModelComp.Deploy();


	}

	protected void DestroyViewModel()
	{
		if ( !ViewModel.IsValid() )
			return;

		ViewModel?.Destroy();
		ViewModel = null;
	}

	protected override void AddedItemToInventory( PlayerInteractionComponent interactionComponent )
	{
		this.GameObject.SetParent( interactionComponent.Player.GameObject, false );
		ToggleWeaponPhysics( false );
		WorldModel.Enabled = false;
		// base.AddedItemToInventory();
	}

	public override void ItemRemovedFromInventory()
	{
		// base.ItemRemovedFromInventory();
		WorldModel.Enabled = true;
		ToggleWeaponPhysics( true );
	}

	public override void EnableItem()
	{
		// BulletPoolingComp.SpawnBulletsAndAddToPool();
		Log.Info( "Enabling Weapon" );
		CreateViewModel();
		// base.EnableItem();
	}

	public override void DisableItem()
	{
		// BulletPoolingComp.ClearPool();
		Log.Info( "Disabling Weapon" );
		DestroyViewModel();
		// base.DisableItem();
	}

	public override bool WillBeDestroyedOnAddToInventory() => false;
	public virtual int TakeAmmo(int count)
	{
		if ( !UsesBullets ) return count;

		if ( Ammo < count ) {
			int newAmmoCount = Math.Max( Ammo - count, 0 );
			int taken = Ammo - newAmmoCount;

			Ammo = newAmmoCount;
			return taken;
		}

		Ammo -= count;

		return count;
	}

	public void StopShoot() => StopPrimaryAttack();
	public void Shoot() => PrimaryAttack();

	public virtual void Aim()
	{
		IsAiming = true;
		ViewModel?.RunEvent<ViewModel>( x =>
		{
			x.Renderer?.Set( "ironsights", 1 );
			x.Renderer?.Set( "ironsights_fire_scale", IronSightsFireScale );
		} );
	}

	public virtual void StopAim()
	{
		ViewModel?.RunEvent<ViewModel>( x =>
		{
			x.Renderer?.Set( "ironsights", 0 );
			x.Renderer?.Set( "ironsights_fire_scale", 1.0f );
		} );
		IsAiming = false;
	}

	
}

public record struct BulletConfiguration
{
	public float Damage { get; set; }
	public float BulletRadius { get; set; }
	public Vector2 AimConeBase { get; set; }
	public Vector2 AimConeSpread { get; set; }
	public float AimConeRecovery { get; set; }
	public Vector2 RecoilPitch { get; set; }
	public Vector2 RecoilYaw { get; set; }
	public float CameraRecoilStrength { get; set; }
	public float CameraRecoilFrequency { get; set; }
	public float Range { get; set; }
}
