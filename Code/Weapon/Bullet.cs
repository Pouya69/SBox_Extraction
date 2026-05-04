using Sandbox;

public sealed class PobxBullet : Component, Component.ICollisionListener
{
	[Property, RequireComponent, Feature( "Movement" )] private Rigidbody Rigidbody { get; set; }
	[Property, Feature( "Movement" )] private Collider Collider { get; set; }
	[Property, Feature( "Movement" )] private float InitialSpeed { get; set; } = 400.0f;
	[Property, Feature( "Movement" )] private float MaxSpeed { get; set; } = 400.0f;
	[Property, Feature( "Movement" ), Group( "Steering and Rotation" )] private bool UpdateRotationTowardsVelocity { get; set; } = false;
	[Property, Feature( "Movement" ), Group( "Steering and Rotation" )] private float RotationInterpSpeed { get; set; } = 6.0f;
	[Property, Feature( "Movement" ), Group( "Steering and Rotation" )] private float VelocityInterpSpeed { get; set; } = 2.0f;
	[Property, Feature( "Movement" ), Group( "Steering and Rotation" )] private float DestroyAfterSeconds { get; set; } = 12.0f;

	private TimeSince BulletLifeTime;

	private Vector3 BulletStartPoint;


	[Property, Feature( "Damage" )] public float Damage { get; set; } = 20.0f;


	private Weapon BelongsToWeapon { get; set; }

	/// <summary>
	/// Used for when we are creating the bullet for the first time.
	/// 
	/// </summary>
	/// <param name="OwnerWeapon"></param>
	public void InitializeBulletFirstTime( Weapon OwnerWeapon )
	{
		DisableBullet();
		BelongsToWeapon = OwnerWeapon;
	}

	/// <summary>
	/// When grabbing from bullet pool.
	/// </summary>
	/// <param name="newPos"></param>
	/// <param name="newRot"></param>
	/// <param name="newConfig"></param>
	public void InitializeBullet( Vector3 newPos, Rotation newRot, BulletConfiguration newConfig )
	{
		WorldPosition = newPos;
		BulletStartPoint = newPos;
		WorldRotation = newRot;

		this.GameObject.Enabled = true;
		this.Enabled = true;
		Rigidbody.Enabled = true;
		Collider.Enabled = true;

		Rigidbody.Sleeping = false;
		Rigidbody.MotionEnabled = true;
		Rigidbody.Velocity = WorldTransform.Forward * InitialSpeed;

		BulletLifeTime = 0.0f;

		Damage = newConfig.Damage;


	}

	public void AddBackToWeaponPool()
	{
		DisableBullet();
		BelongsToWeapon.AddBulletToPool( this );
	}

	public void DisableBullet()
	{
		Rigidbody.MotionEnabled = false;
		Rigidbody.Velocity = Vector3.Zero;
		Rigidbody.Sleeping = true;
		Rigidbody.Enabled = false;
		Collider.Enabled = false;
		this.Enabled = false;
		this.GameObject.Enabled = false;

	}

	protected override void OnUpdate()
	{
		if ( BulletLifeTime >= DestroyAfterSeconds )
		{
			AddBackToWeaponPool();
			return;
		}
	}

	/// <summary>
	/// This is meant to be called continuously, updates the target, rotates slowly to it and moves at a set speed.
	/// Useful for 'tracking RPG life Half Life 2.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="speed"></param>
	[Rpc.Host]
	internal protected void UpdateWithTarget( Vector3 target, float speed )
	{
		var direction = (target - WorldPosition).Normal;
		var targetRotation = Rotation.LookAt( direction, Vector3.Up );

		if ( UpdateRotationTowardsVelocity )
			WorldRotation = Rotation.Slerp( WorldRotation, targetRotation, Time.Delta * RotationInterpSpeed );
		Rigidbody.Velocity = WorldTransform.Forward * (speed * VelocityInterpSpeed);
	}

	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		DamageInfo damageInfo = new(Damage, BelongsToWeapon.GameObject.Parent, BelongsToWeapon.GameObject);
		damageInfo.Position = collision.Contact.Point;
		damageInfo.Origin = BulletStartPoint;
		PobxFunctionLibrary.ApplyDirectionalDamage(damageInfo, collision.Other.GameObject);

		AddBackToWeaponPool();



	}
}
