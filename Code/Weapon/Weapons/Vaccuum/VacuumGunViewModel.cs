using Sandbox;

public sealed class VacuumGunViewModel : ViewModel
{
	public override void OnStopAttack()
	{
		// base.OnStopAttack();
		Renderer?.Set( "b_button", false );
		Renderer?.Set( "stylus", -1.0f );
	}

	public override void OnAttack()
	{
		// base.OnAttack();
		Renderer?.Set( "b_attack", true );
		Renderer?.Set( "b_button", true );
		Renderer?.Set( "stylus", 1.0f );

		DoMuzzleEffect();
		DoEjectBrass();

		if ( IsThrowable )
		{

			Invoke( 0.5f, () =>
			{
				Renderer?.Set( "b_deploy_new", true );
				Renderer?.Set( "b_pull", false );

			} );
		}
	}

	public override void OnSecondaryAttack()
	{
		Renderer?.Set( "stylus", 1.0f );
		Renderer?.Set( "brake", 1.0f );
	}

	public override void OnStopSecondaryAttack()
	{
		Renderer?.Set( "stylus", -1.0f );
		Renderer?.Set( "brake", 0.0f );
	}
}
