using System;
using NPBehave;
using Sandbox.Audio;
using Sandbox.Citizen;
using Action = NPBehave.Action;

public sealed class MyAttemptAI : Component
{
	public Root _behaviorTree { get; set; }

	private Blackboard _blackboard;
	private Clock _clock;

	[Group( "Footsteps" )] [Property] private float FootstepVolume { get; set; } = 1.0f;
	[Group( "Footsteps" )] [Property] private MixerHandle FootstepMixer { get; set; }
	[Group( "Footsteps" )] [Property] private bool EnableFootstepSounds { get; set; } = true;
	[Property] private SkinnedModelRenderer Renderer { get; set; }
	[Property] private CitizenAnimationHelper _anim { get; set; }
	[Property] private SkinnedModelRenderer _modelRenderer;
	[Group( "AI" )] [Property] private NavMeshAgent Agent { get; set; }
	[Group( "AI" )] [Property] private CharacterController AiController { get; set; }
	[Group( "AI" )] [Property] private SplineComponent _PatrolPath { get; set; }
	[Group( "AI" )] [Property] private float WaitTimeBetweenPoints { get; set; } = 5;
	[Group( "AI" )] [Property] private float RotationSpeed { get; set; } = 5;
	[Group( "AI" )] [Property] private PrefabScene WeaponToSpawnWith { get; set; }
	private Weapon CurrentWeaponEquipped { get; set; }
	[Property] private GameObject WeaponAttachmentSocket { get; set; }

	[Property] public ActionSystemComponent ActionSystemComponent;

	private TimeSince _timeSinceStep;

	private bool IsOnGround => AiController?.IsOnGround ?? true;

	private int currentPatrolPathTargetSplineIndex = 0;

	protected override void OnStart()
	{
		_clock = new Clock();
		_blackboard = new Blackboard( _clock );

		this.Renderer.OnFootstepEvent -= this.OnFootstepEvent;
		this.Renderer.OnFootstepEvent += this.OnFootstepEvent;
		// this.ActionSystemComponent.On

		// _blackboard.Set( "MyVector3", new Vector3( 8 ));
		_blackboard.Set( "MyGameObject", GameObject );
		// _blackboard.Set( "ASimpleBool", true);

		_behaviorTree = new Root( _blackboard,
			new Selector(
				PatrolPathsSequence()
			) );
		if ( WeaponToSpawnWith is not null )
			GiveWeapon( WeaponToSpawnWith.Clone().GetComponent<Weapon>() );
		/*_behaviorTree = new Root(_blackboard,
			new Selector(
				new Cooldown( 3f, false, false, true,
					new Action(
						() =>
						{
							_blackboard.Set( "ASimpleBool", !_blackboard.Get<bool>( "ASimpleBool" ) );
						}) {Label = "3 sec Countdown"}),

				new BlackboardCondition( "ASimpleBool", Operator.IsEqual, true, Stops.ImmediateRestart, new Action( () => { Log.Info( "ASimpleBool is true" ); } ) {Label = "ASimpleBool is true"} )
				//new WaitUntilStopped( )
			));
		*/

		_behaviorTree.Start();


	}

	public bool HasWeaponEquipped => CurrentWeaponEquipped is not null;

	public void GiveWeapon( Weapon weapon )
	{
		CurrentWeaponEquipped = weapon;
		CurrentWeaponEquipped.ToggleWeaponPhysics( false );
		CurrentWeaponEquipped.GameObject.Parent = WeaponAttachmentSocket;
		CurrentWeaponEquipped.WorldPosition = WeaponAttachmentSocket.WorldPosition;
		CurrentWeaponEquipped.WorldRotation = WeaponAttachmentSocket.WorldRotation;
		CurrentWeaponEquipped.WorldScale = WeaponAttachmentSocket.WorldScale;

		SwitchToWeaponAnimation( weapon );
	}

	public void SwitchToWeaponAnimation( Weapon weapon )
	{
		_modelRenderer.Set("holdtype", weapon.GetWeaponType().AsInt());
		_modelRenderer.Set("holdtype_handedness", weapon.GetWeaponHoldType().AsInt());
	}

private void OnFootstepEvent( SceneModel.FootstepEvent e )
	{
		if (!this.IsOnGround || !this.EnableFootstepSounds || _timeSinceStep < 0.2f)
			return;
		this._timeSinceStep = (TimeSince) 0.0f; ;
		double volume = e.Volume * this.Agent.WishVelocity.Length.Remap(0.0f, 400f);
		if (volume <= 0.10000000149011612)
			return;
		this.PlayFootstepSound(e.Transform.Position, (float) volume, e.FootId);
	}

	private Surface GroundSurface => this.AiController.GroundCollider?.Surface ?? this.AiController.GroundObject.GetComponent<Collider>().Surface;
	
	private void PlayFootstepSound(Vector3 worldPosition, float volume, int foot)
	{
		if (!this.GroundSurface.IsValid())
			return;
		Log.Info( "FOotstep" );
		SoundEvent sound = foot == 0 ? this.GroundSurface.SoundCollection.FootLeft : this.GroundSurface.SoundCollection.FootRight;
		if (sound == null)
		{
			this.DebugOverlay.Sphere(new Sphere(worldPosition, volume), global::Color.Orange, 10f, overlay: true);
		}
		else
		{
			var soundHandle = this.GameObject.PlaySound(sound, (Vector3) 0.0f);
			if (!soundHandle.IsValid())
				return;
			soundHandle.FollowParent = false;
			soundHandle.TargetMixer = this.FootstepMixer.GetOrDefault();
			soundHandle.Volume *= volume * this.FootstepVolume;
			// this.DebugOverlay.Sphere(new Sphere(worldPosition, volume), duration: 10f, overlay: true);
			// this.DebugOverlay.Text(worldPosition, sound.ResourceName ?? "", 14f, TextFlag.LeftTop, duration: 10f, overlay: true);
		}
	}

	private Sequence PatrolPathsSequence()
	{
		return new Sequence(
			new Action(multiframeFunc2: moveToLocationAI ) {Label = "Move To Next Location"},
			setNextPathAction(),
			new Action( multiframeFunc2: rotateTowards ) {Label = "Rotate Towards Next Position"},
			new Wait( WaitTimeBetweenPoints )
			
		) {Label = "Patrol Paths" };
	}

	private Action.Result moveToLocationAI( Action.Request arg )
	{
		var currentTargetLocation = getTargetPatrolPoint();

		if ( isAtTargetLocation( currentTargetLocation, 16.0f ) )
		{
			return Action.Result.Success;
		}
		
		// Log.Info( "Working..." );
		
		Agent.MoveTo( currentTargetLocation );
		AiController.Move();
		// var directionToTarget = (currentTargetLocation - WorldPosition).Normal * MovementSpeed;
		// AiController.Accelerate( directionToTarget );
		// AiController.Move();
		

		return Action.Result.Progress;
	}

	private Action.Result rotateTowards( Action.Request arg )
	{
		var direction = (getTargetPatrolPoint() - AiController.WorldPosition).WithZ(0).Normal;
		var myForward = AiController.WorldRotation.Forward.WithZ(0);
		if (myForward.Angle(direction) <= 15.0f)
		{
			return Action.Result.Success;
		}
		
		var angleTarget = (MathF.Atan2(direction.y, direction.x)).RadianToDegree();
		AiController.WorldRotation = Rotation.Slerp(AiController.WorldRotation, Rotation.FromYaw( angleTarget ), RotationSpeed * Time.Delta);
		AiController.Move();
		// AiController.Move();
		return Action.Result.Progress;
	}

	private bool isAlreadyFacing(Vector3 direction, float preceision )
	{
		return AiController.WorldRotation.Forward.Angle( direction ) <= preceision;
	}

	private Vector3 getTargetPatrolPoint()
	{
		return _PatrolPath.Spline.GetPoint( currentPatrolPathTargetSplineIndex ).Position + _PatrolPath.WorldPosition;
	}

	private Condition isAtTargetLocationCondition(Vector3 targetLocation, float acceptableRadius, Stops stops, Node decoratee, bool reverseCondition)
	{
		return new Condition(
			() => reverseCondition ? !isAtTargetLocation(targetLocation, acceptableRadius) : isAtTargetLocation(targetLocation, acceptableRadius), stops, decoratee
		);
	}

	private bool isAtTargetLocation(Vector3 targetLocation, float acceptableRadius)
	{
		return Vector3.DistanceBetween( targetLocation, WorldPosition ) <= acceptableRadius;
	}

	private Action setNextPathAction()
	{
		return new Action( SetNextPath ) {Label = "Set Next Path" };
	}

	private void SetNextPath()
	{
		currentPatrolPathTargetSplineIndex++;
		if ( currentPatrolPathTargetSplineIndex >= _PatrolPath.Spline.PointCount )
			currentPatrolPathTargetSplineIndex = 0;
	}
	
	protected override void OnUpdate()
	{
		var delta = Time.Delta;
		_clock.Update( delta );
		base.OnUpdate();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		var direction = (getTargetPatrolPoint() - AiController.WorldPosition).Normal.WithZ(0);
		AiController.Velocity = Agent.Velocity;
		// AiController.Move();
		
		UpdateAnimation(direction, WorldRotation, WorldRotation.Forward);
	}

	private void UpdateAnimation(Vector3 wishVelocity, Rotation rotation, Vector3 lookDirection)
	{
		_anim.WithWishVelocity(  wishVelocity );
		_anim.WithVelocity( Agent.Velocity );
		_anim.AimAngle = rotation;
		_anim.IsGrounded = IsOnGround;
		// Log.Info( Agent. );
		_anim.WithLook( lookDirection );
		_anim.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
	}
}
