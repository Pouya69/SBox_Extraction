using Sandbox;

public sealed class PobxChest : ContainerBase
{
	[Property, Feature( "Properties" )] private SoundEvent SpitSound { get; set; }
	[Property, Feature( "Properties" )] private Vector2 SpitOutStrengthRange { get; set; } = 500.0f;
	[Property, Feature( "Properties" )] private float SpitOutHorizentalRandomness = 30.0f;
	[Property, Feature( "Properties" )] private float SpitOutVerticalRandomness = 30.0f;

	protected override void SpitOut( GameObject gameObject )
	{

		// base.SpitOut( gameObject );
		gameObject.WorldPosition = this.ObjectSpawnPoint.WorldPosition;
		gameObject.WorldRotation = Rotation.Random;

		gameObject.Enabled = true;

		if ( gameObject.GetComponentInChildren<Rigidbody>( true ) is var rb )
		{

			var spitOutVel = this.ObjectSpawnPoint.WorldTransform.Forward
				.WithAimCone( SpitOutHorizentalRandomness, SpitOutVerticalRandomness ) * Random.Shared.Float( SpitOutStrengthRange.x, SpitOutStrengthRange.y);

			rb.ApplyImpulse( spitOutVel );
		}
		else
		{
			Log.Error( gameObject.Name + " has no rigid body. Spawned in chest: " + this.GameObject.Name );
		}

		if ( SpitSound.IsValid() )
		{
			GameObject.PlaySound( SpitSound );
		}

	}
}
