using NPBehave;
using Sandbox.AI;
using Sandbox.Audio;
using Sandbox.Citizen;
using System;
using static NPBehave.Action;
namespace NPBehave;

public enum ECharacterGroundMovementType
{
	WALKING,
	SPRINTING,
	AIMING
}

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
	[Group( "AI" )] [Property] public NavMeshAgent Agent { get; private set; }
	[Group( "AI" )] [Property] public CharacterController AiController { get; private set; }
	[Group( "AI" )] [Property] private SplineComponent _PatrolPath { get; set; }
	[Group( "AI" )] [Property] public float WaitTimeBetweenPoints { get; private set; } = 5;
	[Group( "AI" )] [Property] private PrefabScene WeaponToSpawnWith { get; set; }
	[Group( "AI" )][Property] public EnvironmentQueryHandler AIEnvironmentQueryHandler { get; private set; }
	[Group( "Character Movement" )][Property] public float RotationSpeed { get; private set; } = 5;
	[Group( "Character Movement" )][Property] public float RotationSpeedInCombat { get; private set; } = 30.0f;
	[Group( "Character Movement" )][Property] private float SprintSpeed { get; set; } = 320.0f;
	[Group( "Character Movement" )][Property] private float NormalSpeed { get; set; } = 110.0f;
	[Group( "Character Movement" )] [Property] private float AimingSpeed { get; set; } = 50.0f;
	public ECharacterGroundMovementType CurrentGroundMovementType { get; private set; }
	private Weapon CurrentWeaponEquipped { get; set; }
	[Property] private GameObject WeaponAttachmentSocket { get; set; }

	[Property] public ActionSystemComponent ActionSystemComponent;

	private TimeSince _timeSinceStep;

	private bool IsOnGround => AiController?.IsOnGround ?? true;

	protected override void OnStart()
	{
		_clock = new Clock();
		_blackboard = new Blackboard( _clock );

		this.Renderer.OnFootstepEvent -= this.OnFootstepEvent;
		this.Renderer.OnFootstepEvent += this.OnFootstepEvent;
		// this.ActionSystemComponent.On

		// _blackboard.Set( "MyVector3", new Vector3( 8 ));
		_blackboard.Set( "MyGameObject", GameObject );
		_blackboard.Set( "Is Hostile", false );
		_blackboard.Set( "Current Hostile", GameObject );
		_blackboard.Set( "Target Destination", WorldPosition );
		_blackboard.Set( "Is Moving To Destination", false );
		SetNewFacingObject(GameObject);

		ChangeGroundMovementTypeSprint( ECharacterGroundMovementType.WALKING );
		// _blackboard.Set( "ASimpleBool", true);

		var patrolBehaviour = new ExtractionPatrollingNPCBehaviour( this, GetPointsFromSpine(_PatrolPath.Spline, _PatrolPath.WorldPosition) );
		var hostileBehaviour = new ExtractionHostileBehaviour( this );

		_behaviorTree = new Root( _blackboard,
			new Selector(
				new BlackboardCondition( "Is Hostile", Operator.IsNotEqual, true, Stops.Self, patrolBehaviour ),
				new BlackboardCondition("Is Hostile", Operator.IsEqual, true, Stops.Self, hostileBehaviour)
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

	public Vector3[] GetPointsFromSpine(Spline spline, Vector3 origin = new Vector3()) {
		int count = spline.PointCount;
		var result = new Vector3[count];

		for ( int i = 0; i < count; i++ )
			result[i] = spline.GetPoint( i ).Position + origin;

		return result;
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

	private void RunEnvironmentQuery(EnvironmentQuery query, EEnvQueryResultType resultType, Action<EnvQueryResult> onQueryCompletedFunctor )
	{
		var donutQuery = query as DonutEnvironmentQuery;
		if ( donutQuery is not null)
		{
			AIEnvironmentQueryHandler.RunQuery( donutQuery, resultType, onQueryCompletedFunctor );
		}

		// Support for other query types coming soon... (Box etc.)
	}
	private NPBehave.Action.Result moveToLocationAI( Action.Request arg )
	{
		var currentTargetLocation = GetCurrentTargetLocation();

		if ( isAtTargetLocation( currentTargetLocation, 16.0f ) )
		{
			return NPBehave.Action.Result.Success;
		}
		
		// Log.Info( "Working..." );
		
		Agent.MoveTo( currentTargetLocation );
		AiController.Move();
		// var directionToTarget = (currentTargetLocation - WorldPosition).Normal * MovementSpeed;
		// AiController.Accelerate( directionToTarget );
		// AiController.Move();
		

		return NPBehave.Action.Result.Progress;
	}

	private NPBehave.Action.Result rotateTowards( Action.Request arg )
	{
		var direction = (GetCurrentTargetLocation() - AiController.WorldPosition).WithZ(0).Normal;
		var myForward = AiController.WorldRotation.Forward.WithZ(0);
		if (myForward.Angle(direction) <= 15.0f)
		{
			return NPBehave.Action.Result.Success;
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

	private Condition isAtTargetLocationCondition( Vector3 targetLocation, float acceptableRadius, Stops stops, Node decoratee, bool reverseCondition )
	{
		return new Condition(
			() => reverseCondition ? !isAtTargetLocation( targetLocation, acceptableRadius ) : isAtTargetLocation( targetLocation, acceptableRadius ), stops, decoratee
		);
	}

	private bool isAtTargetLocation( Vector3 targetLocation, float acceptableRadius )
	{
		return Vector3.DistanceBetween( targetLocation, WorldPosition ) <= acceptableRadius;
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
		bool isMoving = IsAIInMovement();
		Vector3 direction = new();
		if ( isMoving )
		{
			direction = (GetCurrentTargetLocation() - AiController.WorldPosition).Normal.WithZ( 0 );
			AiController.Velocity = Agent.Velocity;
		}
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

	public GameObject GetCurrentHostile()
	{
		return (GameObject) _blackboard.Get( "Current Hostile" );
	}

	public bool IsHostile()
	{
		return (bool)_blackboard.Get( "Is Hostile" );
	}

	public void DetectedHostile( GameObject hostileObject, bool shouldAlsoSetFacingDirectionToHostile = true )
	{
		_blackboard.Set( "Is Hostile", true );
		_blackboard.Set( "Current Hostile", hostileObject );

		SetNewFacingObject( hostileObject );
	}

	public void EndHostile()
	{
		_blackboard.Set( "Is Hostile", false );
		_blackboard.Set( "Current Hostile", GameObject );
	}

	public void CrouchBehindCover()
	{
		_anim.DuckLevel = 1.0f;
	}

	public void StandUpFromCover()
	{
		_anim.DuckLevel = 0.0f;
	}

	public void Shoot()
	{
		_modelRenderer.Set( "b_attack", true );
		Log.Info( "SHOOT!" );
	}
	
	public bool IsAIInMovement()
	{
		return (bool) _blackboard.Get( "Is Moving To Destination" );
	}
	public Vector3 GetCurrentTargetLocation()
	{
		return (Vector3) _blackboard.Get( "Target Destination" );
	}

	public Vector3 GetCurrentFacingDirection()
	{
		var facingObject = _blackboard.Get( "Facing Object" );
		if (facingObject is GameObject)
			return (((GameObject)facingObject).WorldPosition - AiController.WorldPosition).WithZ( 0 ).Normal;

		return (((Vector3)facingObject) - AiController.WorldPosition).WithZ( 0 ).Normal;
	}

	public void SetNewTargetLocation( Vector3 newLocation )
	{
		_blackboard.Set( "Is Moving To Destination", true );
		_blackboard.Set( "Target Destination", newLocation );
	}

	public void SetNewFacingObject(Vector3 newFacingLocation)
	{
		_blackboard.Set( "Facing Object", newFacingLocation );
	}

	public void SetNewFacingObject( GameObject newFacingObject )
	{
		_blackboard.Set( "Facing Object", newFacingObject );
	}

	public void ChangeGroundMovementTypeSprint(ECharacterGroundMovementType newGroundMovementType)
	{
		CurrentGroundMovementType = newGroundMovementType;
		switch (newGroundMovementType)
		{
			case ECharacterGroundMovementType.SPRINTING:
				Agent.Acceleration = SprintSpeed;
				Agent.MaxSpeed = SprintSpeed;
				break;

			case ECharacterGroundMovementType.WALKING:
				Agent.Acceleration = NormalSpeed;
				Agent.MaxSpeed = NormalSpeed;
				break;

			case ECharacterGroundMovementType.AIMING:
				Agent.Acceleration = AimingSpeed;
				Agent.MaxSpeed = AimingSpeed;
				break;

			default:
				break;
		}

		
	}

	[Group("Debug")] [Button("HOSTILE!")]
	private void DebugStartHostile()
	{
		DetectedHostile( AIEnvironmentQueryHandler.DebugPlayerRef );
	}

	[Group( "Debug" )] [Button( "PATROL!" )]
	private void DebugEndHostile()
	{
		EndHostile();
	}
}
