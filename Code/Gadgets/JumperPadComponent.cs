using Sandbox;

public sealed class JumperPadComponent : Component, Component.ITriggerListener
{
	/// <summary>
	/// Uses the up vector of the object.
	/// </summary>
	[Property] private float LaunchVelocity = 500.0f;
	[Property] private Vector3 LaunchVelocityAddition = new(0,0,50.0f);
	[Property] private SoundEvent JumpPadSoundFirstTime { get; set; }
	[Property] private SoundEvent JumpPadSoundSecondOrMore { get; set; }

	public bool IsMostlyUpwards => Transform.World.Up.Dot( Vector3.Up ) > 0.5f;

	void ITriggerListener.OnTriggerEnter( GameObject other )
	{
		var entityComponent = other.GetComponentInChildren<IExtractionQuestEntity>(true);
		if ( entityComponent == null)
		{
			return;
		}

		if ( entityComponent.IsUsingJumpPad() )
		{
			// Second time or more on jump pad.
			PlayJumpSound( JumpPadSoundSecondOrMore );
		}
		else
		{
			// First time
			PlayJumpSound( JumpPadSoundFirstTime );
		}

		if ( IsMostlyUpwards )
			entityComponent.LaunchEntity( (LaunchVelocity * Transform.World.Up) + LaunchVelocityAddition );
		else
			entityComponent.LaunchEntity( (LaunchVelocity * 1.5f * Transform.World.Up) + LaunchVelocityAddition );
	}

	private void PlayJumpSound( SoundEvent soundToPlay )
	{
		if ( soundToPlay.IsValid() )
		{
			var soundSpawned = GameObject.PlaySound( soundToPlay );
			var player = GameObject.GetComponentInChildren<PobxPlayer>( true );
			if ( player.IsValid() && player.IsLocalPlayer && soundSpawned.IsValid() )
			{
				soundSpawned.SpacialBlend = 0.0f;
			}
		}
	}
}
