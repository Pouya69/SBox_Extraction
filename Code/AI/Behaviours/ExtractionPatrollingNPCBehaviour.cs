using NPBehave;
using System;
namespace NPBehave;

public class ExtractionPatrollingNPCBehaviour : BaseBehaviourTree
{
	public Vector3[] PatrolPoints;
	public int PatrolPointsCount { get; private set;  }
	public int currentPatrolPathTargetSplineIndex = 0;

	public ExtractionPatrollingNPCBehaviour( MyAttemptAI owner, Vector3[] patrolPositions) : base(owner)
	{

		Owner.SetNewTargetLocation( patrolPositions[0] );
		this.InitComposite( [PatrolPathsSequence()] );


		PatrolPoints = patrolPositions;
		PatrolPointsCount = patrolPositions.Length;
	}

	protected Vector3 getTargetPatrolPoint()
	{
		return PatrolPoints[currentPatrolPathTargetSplineIndex];
	}

	protected Action setNextPathAction()
	{
		return new Action( SetNextPath ) { Label = "Set Next Path" };
	}

	protected void SetNextPath()
	{
		currentPatrolPathTargetSplineIndex++;
		if ( currentPatrolPathTargetSplineIndex >= PatrolPointsCount )
			currentPatrolPathTargetSplineIndex = 0;

		var newLocation = getTargetPatrolPoint();
		Owner.SetNewTargetLocation( newLocation );
		Owner.SetNewFacingObject( newLocation );
	}

	private Sequence PatrolPathsSequence()
	{
		return new Sequence(
			new Action( multiframeFunc2: moveToLocationAI ) { Label = "Move To Next Location" },
			setNextPathAction(),
			new Action( multiframeFunc2: rotateTowardsNextPatrolPath ) { Label = "Rotate Towards Next Position" },
			new Wait( Owner.WaitTimeBetweenPoints )
		)
		{ Label = "Patrol Paths" };
	}

	protected Action.Result rotateTowardsNextPatrolPath( Action.Request arg )
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
}
