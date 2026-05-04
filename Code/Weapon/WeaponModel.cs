using Sandbox;
using System.Diagnostics;

public class WeaponModel : Component
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public GameObject MuzzleTransform { get; set; }
	[Property] public GameObject EjectTransform { get; set; }
	[Property, Group( "References to other prefabs" )] public PrefabScene MuzzleEffect { get; set; }
	[Property, Group( "References to other prefabs" )] public PrefabScene EjectBrass { get; set; }
	[Property, Group( "References to other prefabs" )] public PrefabScene TracerEffect { get; set; }

	public void Deploy()
	{
		Renderer?.Set( "b_deploy", true );
	}

	public Transform GetTracerOrigin()
	{
		if ( MuzzleTransform.IsValid() )
			return MuzzleTransform.WorldTransform;

		return WorldTransform;
	}

	public void DoTracerEffect( Vector3 hitPoint, Vector3? origin = null )
	{
		throw new NotImplementedException();
		/*
		if ( !TracerEffect.IsValid() ) return;

		var tracerOrigin = GetTracerOrigin().WithScale( 1 );
		if ( origin.HasValue ) tracerOrigin = tracerOrigin.WithPosition( origin.Value );

		var effect = TracerEffect.Clone( new CloneConfig { Transform = tracerOrigin, StartEnabled = true } );

		if ( effect.GetComponentInChildren<Tracer>() is Tracer tracer )
		{
			tracer.EndPoint = hitPoint;
		}
		*/
	}

	public void DoEjectBrass()
	{
		if ( !EjectBrass.IsValid() ) return;
		if ( !EjectTransform.IsValid() ) return;

		var effect = EjectBrass.Clone( new CloneConfig { Transform = EjectTransform.WorldTransform.WithScale( 1 ), StartEnabled = true } );
		effect.WorldRotation = effect.WorldRotation * new Angles( 90, 0, 0 );

		var ejectDirection = (EjectTransform.WorldRotation.Forward * 250 + (EjectTransform.WorldRotation.Right + Vector3.Random * -0.35f) * 250);

		var rb = effect.GetComponentInChildren<Rigidbody>();
		rb.Velocity = ejectDirection;
		rb.AngularVelocity = EjectTransform.WorldRotation.Right * 50f;
	}

	public void DoMuzzleEffect()
	{
		if ( !MuzzleEffect.IsValid() ) return;
		if ( !MuzzleTransform.IsValid() ) return;

		MuzzleEffect.Clone( new CloneConfig { Parent = MuzzleTransform, Transform = global::Transform.Zero, StartEnabled = true } );
	}

	public virtual void OnAttack()
	{

	}

	public virtual void OnStopAttack() { }

	public virtual void CreateRangedEffects( Weapon weapon, Vector3 hitPoint, Vector3? origin )
	{

	}
}
