using NPBehave;
using Sandbox;
using Sandbox.Navigation;
using Sandbox.VR;
using System;
namespace NPBehave;

public class BaseBehaviourTree : Selector
{
	public MyAttemptAI Owner;

	public BaseBehaviourTree( MyAttemptAI owner ) : base() {
		Owner = owner;
		
		// this.InitComposite( children );
	}

	protected Action.Result moveToLocationAI( Action.Request arg )
	{
		if ( this.IsStopRequested )
			return Action.Result.Failed;

		var currentTargetLocation = Owner.GetCurrentTargetLocation();

		if ( isAtTargetLocation( currentTargetLocation, 50.0f ) )
		{
			return Action.Result.Success;
		}

		// Log.Info( "Working..." );

		Owner.Agent.MoveTo( currentTargetLocation );
		Owner.AiController.Move();
		// var directionToTarget = (currentTargetLocation - WorldPosition).Normal * MovementSpeed;
		// AiController.Accelerate( directionToTarget );
		// AiController.Move();


		return Action.Result.Progress;
	}

	protected Action.Result rotateTowardsAction( Action.Request arg )
	{
		var direction = Owner.GetCurrentFacingDirection();
		var myForward = Owner.AiController.WorldRotation.Forward.WithZ( 0 );
		if ( myForward.Angle( direction ) <= 15.0f )
		{
			return Action.Result.Success;
		}

		var angleTarget = (MathF.Atan2( direction.y, direction.x )).RadianToDegree();
		Owner.AiController.WorldRotation = Rotation.Slerp( Owner.AiController.WorldRotation, Rotation.FromYaw( angleTarget ), Owner.RotationSpeed * Time.Delta );
		Owner.AiController.Move();
		// AiController.Move();
		return Action.Result.Progress;
	}

	protected void rotateTowardsInCombat()
	{
		var direction = Owner.GetCurrentFacingDirection();
		var myForward = Owner.AiController.WorldRotation.Forward.WithZ( 0 );

		var angleTarget = (MathF.Atan2( direction.y, direction.x )).RadianToDegree();
		Owner.AiController.WorldRotation = Rotation.Slerp( Owner.AiController.WorldRotation, Rotation.FromYaw( angleTarget ), Owner.RotationSpeedInCombat * Time.Delta );
		Owner.AiController.Move();
	}

	protected void rotateTowards()
	{
		var direction = Owner.GetCurrentFacingDirection();
		var myForward = Owner.AiController.WorldRotation.Forward.WithZ( 0 );

		var angleTarget = (MathF.Atan2( direction.y, direction.x )).RadianToDegree();
		Owner.AiController.WorldRotation = Rotation.Slerp( Owner.AiController.WorldRotation, Rotation.FromYaw( angleTarget ), Owner.RotationSpeed * Time.Delta );
		Owner.AiController.Move();
	}

	protected bool isAlreadyFacing( Vector3 direction, float preceision )
	{
		return Owner.AiController.WorldRotation.Forward.Angle( direction ) <= preceision;
	}

	protected Condition isAtTargetLocationCondition( Vector3 targetLocation, float acceptableRadius, Stops stops, Node decoratee, bool reverseCondition )
	{
		return new Condition(
			() => reverseCondition ? !isAtTargetLocation( targetLocation, acceptableRadius ) : isAtTargetLocation( targetLocation, acceptableRadius ), stops, decoratee
		);
	}

	protected bool isAtTargetLocation( Vector3 targetLocation, float acceptableRadius )
	{
		return Vector3.DistanceBetween( targetLocation, Owner.WorldPosition ) <= acceptableRadius;
	}

	protected void RunEnvironmentQuery( EnvironmentQuery query, EEnvQueryResultType resultType, Action<EnvQueryResult> onQueryCompletedFunctor )
	{
		var donutQuery = query as DonutEnvironmentQuery;
		if ( donutQuery is not null )
		{
			Owner.AIEnvironmentQueryHandler.RunQuery( donutQuery, resultType, onQueryCompletedFunctor );
		}

		// Support for other query types coming soon... (Box etc.)
	}


	protected GameObject GetCurrentHostile()
	{
		return (GameObject) Blackboard.Get( "Current Hostile" );
	}

	protected void DetectedHostile(GameObject hostileObject)
	{
		Blackboard.Set( "Current Hostile", hostileObject );
	}

	protected void FindRandomLocation(float radius = 200.0f)
	{
		var result = Owner.Scene.NavMesh.GetRandomPoint( Owner.WorldPosition, radius);
		if (result.HasValue)
			Owner.SetNewTargetLocation( result.Value );

		//return result;
	}

	protected Service FaceTowardsTargetService(Node action) {
		return new Service(new System.Action( rotateTowards ), action
		);
	}

	protected Service FaceTowardsInCombatTargetService( Node action )
	{
		return new Service(new System.Action( rotateTowardsInCombat ), action
		);
	}


	protected void StartSprinting()
	{
		Owner.ChangeGroundMovementTypeSprint( ECharacterGroundMovementType.SPRINTING );
	}

	protected void StartNormalWalking()
	{
		Owner.ChangeGroundMovementTypeSprint( ECharacterGroundMovementType.WALKING );
	}

	protected void StartAimWalking()
	{
		Owner.ChangeGroundMovementTypeSprint( ECharacterGroundMovementType.AIMING );
	}
}
