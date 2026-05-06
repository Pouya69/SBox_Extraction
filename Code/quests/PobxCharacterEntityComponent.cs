using Sandbox;

public class PobxCharacterEntityComponent : ExtractionQuestEntityComponent
{
	[Property] private CharacterController Controller { get; set; }

	public bool IsCharacterUsingJumpPad { get; protected set; } = false;

	protected override void OnAwake()
	{
		GameObject.GetComponent<SourceMovement>( true )?.OnLanded += LandedOnGround;
	}

	protected override void OnDestroy()
	{
		GameObject.GetComponent<SourceMovement>( true )?.OnLanded -= LandedOnGround;
	}

	public void LandedOnGround()
	{
		IsCharacterUsingJumpPad = false;
	}

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

		IsCharacterUsingJumpPad = true;
	}

	public override void ToggleEnablePhysics( bool enable )
	{
		// base.ToggleEnablePhysics( enable );
	}

	public override bool CanBeRemoteGrabbed() => false;

	public override bool IsUsingJumpPad() => IsCharacterUsingJumpPad;
}
