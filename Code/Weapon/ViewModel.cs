using Sandbox;
using System.Diagnostics;

public sealed class ViewModel : WeaponModel, ICameraSetup
{

	/// <summary>
	/// For the animations affecting the camera directly.
	/// </summary>
	[Property, Group( "Animation" )] float CameraAnimationScale { get; set; } = 0.5f;

	/// <summary>
	/// If true, it is Terry.
	/// </summary>
	[Property, Group( "Animation" )]
	public bool IsCitizen { get; set; } = false;

	/// <summary>
	/// For grenades etc.
	/// </summary>
	[Property, Group( "Animation" )]
	public bool IsThrowable { get; set; } = false;


	[Property, Group( "Animation" )] float AnimationSpeed = 1.0f;

	/// <summary>
	/// For things like shotgun
	/// </summary>
	[Property, Group( "Animation" )] bool IsIncremental = false;

	/// <summary>
	/// Animation speed for incremental reload sections.
	/// </summary>
	[Property, Group( "Animation" )] float IncrementalAnimationSpeed = 5.0f;

	/// <summary>
	/// Use fast anims?
	/// </summary>
	[Property, Group( "Animation" )]
	public bool UseFastAnimations { get; set; } = false;

	[Property, Group( "Animation" )] float MoveBobScale = 2.0f;


	/// <summary>
	/// Staggered recoil for continuous fire
	/// </summary>
	[Property, Group( "Animation" )] float AnimationRecoilScale = 0.5f;

	/// <summary>
	/// How much inertia should this weapon have?
	/// X is pitch, Y is yaw
	/// </summary>
	[Property, Group( "Inertia" )]
	Vector2 InertiaScale { get; set; } = new Vector2( 2, 2 );

	public SourceMovement Controller { get; set; }

	Vector2 lastInertia;
	Vector2 currentInertia;
	bool isFirstUpdate = true;

	bool isAttacking = false;
	TimeSince AttackDuration;

	bool isFinishingReload = false;
	TimeSince reloadFinishTimer;


	protected override void OnStart()
	{
		Renderer.Set( "skeleton", IsCitizen ? 1 : 0 );

		foreach ( var renderer in GetComponentsInChildren<ModelRenderer>() )
		{
			// Don't render shadows for viewmodels
			renderer.RenderType = ModelRenderer.ShadowRenderType.Off;
		}
	}

	protected override void OnAwake()
	{
		// HandsViewModelObject.Renderer.BoneMergeTarget = this.Renderer;
	}

	protected override void OnUpdate()
	{
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		if ( !Controller.IsValid() )
			return;

		var rot = Scene.Camera.WorldRotation.Angles();
		var velocity = Controller.Controller.Velocity;
		var velocityLength = velocity.Length;
		var forward = Scene.Camera.WorldRotation.Forward.Dot(velocity);
		var right = Scene.Camera.WorldRotation.Right.Dot( velocity );

		var directionOfMovement = MathF.Atan2( right, forward ).RadianToDegree().NormalizeDegrees();

		Renderer.Set( "b_twohanded", true);
		Renderer.Set( "deploy_type", UseFastAnimations ? 1 : 0 );
		Renderer.Set( "reload_type", UseFastAnimations ? 1 : 0 );

		Renderer.Set( "b_grounded", Controller.Controller.IsOnGround );

		Renderer.Set( "aim_yaw", rot.yaw );
		Renderer.Set( "aim_yaw_inertia", currentInertia.y * InertiaScale.y);

		Renderer.Set( "aim_pitch_inertia", currentInertia.x * InertiaScale.x );
		Renderer.Set( "aim_pitch", rot.pitch );

		Renderer.Set( "move_bob", velocityLength.Remap(0.0f, Controller.RunSpeed * MoveBobScale, 0.0f, 1.0f));

		Renderer.Set( "attack_hold", isAttacking ? (AttackDuration.Relative * AnimationRecoilScale).Clamp(0.0f, 1.0f) : 0);

		Renderer.Set( "move_direction", directionOfMovement );
		Renderer.Set( "move_speed", velocityLength );
		Renderer.Set( "move_groundspeed", velocity.WithZ(0).Length );
		Renderer.Set( "move_x", forward );
		Renderer.Set( "move_y", right );
		Renderer.Set( "move_z", velocity.z );

		if ( isFinishingReload && reloadFinishTimer >= 0.5f )
		{
			isFinishingReload = false;
			Renderer.Set( "speed_reload", AnimationSpeed );
			Renderer.Set( "b_reloading", false );
		}
	}

	private void ApplyInertia()
	{
		var rot = Scene.Camera.WorldRotation.Angles();

		if ( isFirstUpdate )
		{
			lastInertia = new(rot.pitch, rot.yaw);
			currentInertia = Vector2.Zero;
			isFirstUpdate = false;
		}

		currentInertia = new(Angles.NormalizeAngle(rot.pitch - lastInertia.x), Angles.NormalizeAngle(rot.yaw - lastInertia.y));
		lastInertia = new( rot.pitch, rot.yaw );
	}

	private void ApplyAnimationTransform( CameraComponent cameraComp )
	{
		if ( !Renderer.IsValid() ) return;

		if (Renderer.TryGetBoneTransformLocal("camera", out var bone))
		{
			cameraComp.WorldPosition += cameraComp.WorldRotation * bone.Position * CameraAnimationScale;
			cameraComp.WorldRotation *= bone.Rotation * CameraAnimationScale;
		}
	}

	void ICameraSetup.Setup( CameraComponent cameraComp )
	{
		// Renderer?.Set();

		WorldPosition = cameraComp.WorldPosition;
		WorldRotation = cameraComp.WorldRotation;


		ApplyInertia();
		ApplyAnimationTransform( cameraComp );
	}

	public void OnReloadStart()
	{
		// Log.Info( "Reload" );
		isFinishingReload = false;
		Renderer?.Set( "speed_reload", AnimationSpeed );
		Renderer?.Set( IsIncremental ? "b_reloading" : "b_reload", true );
	}

	/// <summary>
	/// e.g. On shotgun shell in.
	/// </summary>
	public void OnIncrementalReload()
	{
		Renderer?.Set( "speed_reload", IncrementalAnimationSpeed );
		Renderer?.Set( "b_reloading_shell", true );
	}

	public void OnReleoadFinish()
	{
		if (IsIncremental)
		{
			isFinishingReload = true;
			reloadFinishTimer = 0.0f;
		}
		else
		{
			Renderer?.Set( "b_reload", false );
		}
	}

	public override void OnStopAttack()
	{
		// base.OnStopAttack();
	}

	public override void OnAttack()
	{
		// base.OnAttack();
		Renderer?.Set( "b_attack", true );

		DoMuzzleEffect();
		DoEjectBrass();

		if ( IsThrowable )
		{
			Renderer?.Set( "b_throw", true );

			Invoke( 0.5f, () =>
			{
				Renderer?.Set( "b_deploy_new", true );
				Renderer?.Set( "b_pull", false );

			} );
		}
	}
}
