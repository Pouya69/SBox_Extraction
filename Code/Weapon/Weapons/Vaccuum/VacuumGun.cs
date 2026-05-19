using Sandbox;

public sealed class VacuumGun : Weapon
{
	private enum EVacuumGunMode
	{
		SUCKING,
		SPITTING
	}

	[Property, Feature( "Sucking" )] private float VacuumStrength = 300.0f;
	[Property, Feature( "Sucking" )] private float VacuumScanRadius = 100.0f;
	[Property, Feature( "Sucking" )] private float VacuumScanDistance = 300.0f;
	[Property, Feature( "Sucking" )] private float VacuumSuckedDistanceThreshold = 40.0f;
	[Property, Feature( "Sucking" )] private float VacuumScanForwardOffset = 15.0f;


	[Property, Feature( "Sucking" )] private string ScanCollisionTag { get; set; } = "interaction";

	[Property, Feature( "Spit Out" )] private float SpitOutStrength = 600.0f;

	private Stack<IExtractionQuestEntity> SuckedEntities = new();

	private EVacuumGunMode CurrentVacuumMode = EVacuumGunMode.SUCKING;

	protected override void OnAwake()
	{
		base.OnAwake();
		Ammo = 0;
	}

	protected override void PrimaryAttack()
	{
		StopSecondaryAttack();

		ViewModel?.RunEvent<ViewModel>( x => x.OnAttack() );
		CurrentVacuumMode = EVacuumGunMode.SUCKING;
		if ( IsVacuumFull() && CurrentVacuumMode == EVacuumGunMode.SUCKING )
		{
			CannotSuckVacuumFull();
			return;
		}

		base.PrimaryAttack();
	}

	protected override bool CanPrimaryAttack()
	{
		return !IsVacuumFull();
	}

	protected override bool CanSecondaryAttack()
	{
		return HasAmmo();
	}

	public override void Reload()
	{
		base.Reload();
		ChangeFireMode();
	}

	protected override void OnUpdate()
	{
		if ( !IsShooting ) return;

		if (CurrentVacuumMode == EVacuumGunMode.SUCKING)
		{
			if ( IsVacuumFull() )
			{
				Log.Info( "FULL" );
				IsShooting = false;
				CannotSuckVacuumFull();
				return;
			}
		}
		else if (CurrentVacuumMode == EVacuumGunMode.SPITTING)
		{
			CurrentFireTime += Time.Delta;

			if ( CurrentFireTime < FireRate ) return;
		}
		

		ShootWeapon();
	}

	public override void OnControl( PobxPlayer Player )
	{
		if ( CanReload() )
		{
			if ( Input.Pressed( "Reload" ) )
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
				StopSecondaryAttack();
			else
				StopAim();
		}
	}

	protected override void ShootWeapon()
	{
		// base.ShootWeapon();
		CurrentFireTime = 0.0f;
		switch (CurrentVacuumMode)
		{
			case EVacuumGunMode.SUCKING:
				Suck();
				break;

			case EVacuumGunMode.SPITTING:
				Spit();
				break;
		}
	}

	private void Suck()
	{
		Vector3 forward = AimRay.Forward;
		Vector3 start = CurrentViewModelComp.GetTracerOrigin().Position + forward * VacuumScanForwardOffset;
		Vector3 end = start + VacuumScanDistance * forward;
		if ( DebugWeaponActions )
		{
			// Log.Info( "Sucking..." );
			DebugOverlay.Sphere( new Sphere( start, VacuumScanRadius ), Color.Red );
			DebugOverlay.Sphere( new Sphere( end, VacuumScanRadius ), Color.Red );
		}

		var traceData = Scene.Trace.Sphere( VacuumScanRadius, start, end ).WithTag( ScanCollisionTag ).IgnoreGameObjectHierarchy( this.GameObject.Parent );
		if ( HasOwner )
			traceData.IgnoreGameObjectHierarchy( this.Owner.GameObject );
		var results = traceData.RunAll();

		foreach ( var item in results )
		{
			var entity = item.GameObject.GetComponent<IExtractionQuestEntity>();
			if (entity == null)
				continue;
			if (!entity.CanBeRemoteGrabbed() )
				continue;

			if ( DebugWeaponActions )
			{
				DebugOverlay.Sphere( new Sphere( item.GameObject.WorldPosition, 30.0f ), Color.Blue );
			}

			entity.GetRigidbody().MotionEnabled = true;

			if (item.GameObject.WorldPosition.DistanceSquared(start) <= VacuumSuckedDistanceThreshold * VacuumSuckedDistanceThreshold )
			{
				SuckedObject( entity );
				if ( IsVacuumFull() ) return;
				continue;
			}

			Vector3 suckingVel = (start - item.GameObject.WorldPosition).Normal * VacuumStrength;
			entity.LaunchEntity( suckingVel, false );
		}
	}

	private void Spit()
	{

		if ( !HasAmmo() )
		{
			SpitOutFailedEmptyVacuum();
			Log.Info( "Empty" );
			return;
		}

		if ( DebugWeaponActions )
		{
			Log.Info( "Spitting..." );
		}

		ViewModel?.RunEvent<ViewModel>( x => x.OnSecondaryAttack() );
		Invoke( FireRate / 4, () => {
			ViewModel?.RunEvent<ViewModel>( x => x.OnStopSecondaryAttack() );
		} );
		
		IExtractionQuestEntity objectToSpitOut = SuckedEntities.Pop();
		Ammo--;
		PrepareEntityForSpitOut(objectToSpitOut);
		objectToSpitOut.LaunchEntity( SpitOutStrength * AimRay.Forward, true );
		ApplyRecoil();
		PlayShootSound();
	}


	private void SuckedObject( IExtractionQuestEntity objectSucked) {
		objectSucked.ToggleEnablePhysics( false );
		objectSucked.GetRenderer().Enabled = false;
		objectSucked.GetGameObject().SetParent( this.ViewModel );
		objectSucked.GetGameObject().WorldPosition = this.CurrentViewModelComp.GetTracerOrigin().Position;
		objectSucked.GetGameObject().WorldRotation = this.CurrentViewModelComp.GetTracerOrigin().Rotation;
		Ammo++;
		SuckedEntities.Push( objectSucked );
	}

	private void PrepareEntityForSpitOut(IExtractionQuestEntity entity)
	{
		entity.ToggleEnablePhysics( true );
		entity.GetGameObject().SetParent( null );
		entity.GetGameObject().WorldPosition = this.CurrentViewModelComp.GetTracerOrigin().Position;
		entity.GetGameObject().WorldRotation = this.CurrentViewModelComp.GetTracerOrigin().Rotation;
		entity.GetRenderer().Enabled = true;
	}

	

	protected override void SecondaryAttack()
	{
		StopPrimaryAttack();
		CurrentVacuumMode = EVacuumGunMode.SPITTING;

		ChangedToSuckMode();

		base.SecondaryAttack();
		// ShootWeapon();
	}

	public override void StopPrimaryAttack()
	{
		ViewModel?.RunEvent<ViewModel>( x => x.OnStopAttack() );
		base.StopPrimaryAttack();
	}

	public override void StopSecondaryAttack()
	{
		base.StopSecondaryAttack();
		ViewModel?.RunEvent<ViewModel>( x => x.OnStopSecondaryAttack() );
	}

	private void ChangedToSuckMode()
	{

	}

	private void ChangedToSpitOutMode()
	{

	}


	private void SpitOutFailedEmptyVacuum()
	{

	}

	private void CannotSuckVacuumFull()
	{

	}
	public bool IsVacuumFull() => Ammo == MaxBullets;
}
