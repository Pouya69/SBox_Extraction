using Sandbox;

public sealed class VacuumGun : Weapon
{
	private enum EVacuumGunMode
	{
		SUCKING,
		SPITTING
	}

	[Property, Feature( "Sucking" )] private int GunStorageCapacity = 5;
	[Property, Feature( "Sucking" )] private float VacuumStrength = 300.0f;
	[Property, Feature( "Sucking" )] private float VacuumScanRadius = 100.0f;
	[Property, Feature( "Sucking" )] private float VacuumScanDistance = 300.0f;
	[Property, Feature( "Sucking" )] private float VacuumSuckedDistanceThreshold = 40.0f;
	[Property, Feature( "Sucking" )] private float VacuumScanForwardOffset = 15.0f;


	[Property, Feature( "Sucking" )] private string ScanCollisionTag { get; set; } = "interaction";

	[Property, Feature( "Spit Out" )] private float SpitOutStrength = 600.0f;
	[Property, Feature( "Spit Out" )] private float SpitOutFireRate = 20.0f;

	private Stack<IExtractionQuestEntity> SuckedEntities = new();

	private float CurrentFireTime;

	private EVacuumGunMode CurrentVacuumMode = EVacuumGunMode.SUCKING;

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	public override void Shoot()
	{
		CurrentFireTime = 0.0f;
		if ( IsVacuumFull() && CurrentVacuumMode == EVacuumGunMode.SUCKING )
		{
			CannotSuckVacuumFull();
			return;
		}

		base.Shoot();
	}

	public override void Reload()
	{
		base.Reload();
		ChangeFireMode();
	}

	public override void StopShoot()
	{
		base.StopShoot();
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

			if ( CurrentFireTime < 1.0f / SpitOutFireRate ) return;
		}
		

		ShootWeapon();

		CurrentFireTime = 0.0f;
	}

	protected override void ShootWeapon()
	{
		base.ShootWeapon();

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

		Vector3 forward = MuzzleSocket.WorldTransform.Forward;
		Vector3 start = MuzzleSocket.WorldPosition + forward * VacuumScanForwardOffset;
		Vector3 end = start + VacuumScanDistance * forward;

		if ( DebugWeaponActions )
		{
			Log.Info( "Sucking..." );
			DebugOverlay.Sphere( new Sphere( start, VacuumScanRadius ), Color.Red );
			DebugOverlay.Sphere( new Sphere( end, VacuumScanRadius ), Color.Red );
		}

		var results = Scene.Trace.Sphere( VacuumScanRadius, start, end).WithTag( ScanCollisionTag ).IgnoreGameObjectHierarchy(this.GameObject).RunAll();
		foreach ( var item in results )
		{
			var entity = item.GameObject.GetComponent<IExtractionQuestEntity>();
			if ( entity == null || !entity.CanBeRemoteGrabbed() )
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

		if ( IsVacuumEmpty() )
		{
			SpitOutFailedEmptyVacuum();
			return;
		}

		if ( DebugWeaponActions )
		{
			Log.Info( "Spitting..." );
		}

		IExtractionQuestEntity objectToSpitOut = SuckedEntities.Pop();
		PrepareEntityForSpitOut(objectToSpitOut);
		objectToSpitOut.LaunchEntity( SpitOutStrength * MuzzleSocket.WorldTransform.Forward, true );
	}


	private void SuckedObject( IExtractionQuestEntity objectSucked) {
		objectSucked.ToggleEnablePhysics( false );
		objectSucked.GetRenderer().Enabled = false;
		objectSucked.GetGameObject().SetParent( this.GameObject );
		SuckedEntities.Push( objectSucked );
	}

	private void PrepareEntityForSpitOut(IExtractionQuestEntity entity)
	{
		entity.ToggleEnablePhysics( true );
		entity.GetRenderer().Enabled = true;
		entity.GetGameObject().SetParent( null );
		entity.GetGameObject().WorldPosition = MuzzleSocket.WorldPosition;
		entity.GetGameObject().WorldRotation = MuzzleSocket.WorldRotation;
	}

	public override void ChangeFireMode()
	{
		CurrentVacuumMode = (EVacuumGunMode) (CurrentVacuumMode.AsInt() + 1 > 1 ? 0 : CurrentVacuumMode.AsInt() + 1);
		switch (CurrentVacuumMode)
		{
			case EVacuumGunMode.SUCKING:
				ChangedToSuckMode();
				break;

			case EVacuumGunMode.SPITTING:
				ChangedToSpitOutMode();
				break;
		}
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

	public bool IsVacuumFull() => SuckedEntities.Count == GunStorageCapacity;
	public bool IsVacuumEmpty() => SuckedEntities.Count == 0;
}
