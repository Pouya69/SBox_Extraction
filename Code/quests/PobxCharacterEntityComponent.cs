using Sandbox;

public class PobxCharacterEntityComponent : ExtractionQuestEntityComponent
{
	[Property] private CharacterController Controller { get; set; }

	public override void LaunchEntity( Vector3 velocity, bool ignoreMass = true )
	{
		if ( ignoreMass )
		{
			Controller.Velocity = Controller.Velocity.WithZ( 0 );
			Controller.Punch( velocity );
		}
		else
		{
			Controller.Velocity = Controller.Velocity.WithZ( 0 );
			Controller.Punch( velocity );
		}
	}

	public override void ToggleEnablePhysics( bool enable )
	{
		// base.ToggleEnablePhysics( enable );
	}

	public override bool CanBeRemoteGrabbed() => false;
}
