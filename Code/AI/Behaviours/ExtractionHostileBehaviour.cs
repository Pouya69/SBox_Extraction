using NPBehave;
using Sandbox;
using System;
namespace NPBehave;

public class ExtractionHostileBehaviour : BaseBehaviourTree
{
	public ExtractionHostileBehaviour( MyAttemptAI owner ) : base( owner )
	{
		this.InitComposite( [
			new Sequence(
				FaceTowardsInCombatTargetService(ShootingSequence()),
				new RandomSelector(
					new Sequence(
						new Cooldown(20.0f, true, false, true, FaceTowardsInCombatTargetService(MoveToCover())) {Label = "Cooldown of Cover."},
						FaceTowardsInCombatTargetService(new Repeater(2, new Sequence(
							TakeCoverAndStand(),
							ShootingSequence()
						)))
					),
					FaceTowardsInCombatTargetService(MoveRandomlyAndShoot())
				)
				
			) {Label = "Shoot and then move to cover."},
		] );
	}

	private Service MoveRandomlyAndShoot()
	{
		return new Service( 3.0f, 1.0f, new System.Action( FindRandomLocationWhileShooting ),
			new Parallel(Parallel.Policy.One, Parallel.Policy.One, [
				new Action( moveToLocationAI ) { Label = "Move to Random Locaton" },
				 ShootingSequenceLong()
			] )
		);
	}

	private void FindRandomLocationWhileShooting() => FindRandomLocation( 200.0f );

	private Sequence MoveToCover()
	{

		return new Sequence(
			new Action( multiframeFunc2: FindCover ) { Label = "Finding Cover" },
			new Action( moveToLocationAI ) { Label = "Move to Cover" },
			new Wait(0.5f)
		//new Action()
		)
		{ Label = "Move To Cover" };
	}

	private Sequence TakeCoverAndStand()
	{
		return new Sequence(
			new Action( CrouchBehindCover ),
			new Wait( 3.0f, 1.5f ),
			new Action( StandFromCover )
		) { Label = "Take Cover"};
	}

	private void ShootHostile()
	{
		Owner.Shoot();
	}

	private Repeater ShootingSequence()
	{
		var rand = new System.Random();
		int count = rand.Next( 3, 5 );
		return new Repeater( count, new Sequence(
				new Action(ShootAction) { Label = "Shoot" },
				new Wait(0.4f, 0.15f) { Label = "Random Wait" }
			) { Label = "Shooting Sequence" }
		)
		{ Label = "Shooting Loop"};
	}

	private Repeater ShootingSequenceLong()
	{
		var rand = new System.Random();
		int count = rand.Next( 5, 7 );
		return new Repeater( count, new Sequence(
				new Action( ShootAction ) { Label = "Shoot" },
				new Wait( 0.5f, 0.3f ) { Label = "Random Wait"}
			)
		{ Label = "Shooting Sequence" }
		)
		{ Label = "Shooting Loop Long" };
	}

	private Action.Result FindCover( Action.Request arg )
	{
		if ( Owner.AIEnvironmentQueryHandler.QueryStatus == Action.Result.Progress )
		{
			// EQS Running.
			return Action.Result.Progress;
		}

		if ( arg == Action.Request.Start )
		{
			// Start it.
			var currentHostile = GetCurrentHostile();
			DonutEnvironmentQuery query;
			if ( currentHostile is null )
			{
				// Go take cover around yourself
				query = new DonutEnvironmentQuery( Owner.GameObject, Owner.GameObject, Owner.WorldPosition, 2000.0f, 0.0f, 110.0f, 15.0f, EEnvQueryAxisType.ONLY_XY, "ai_cover"
				, [new EEnvQueryFilter( EEnvQueryFilterType.COLLISION, true ), new EEnvQueryFilter( EEnvQueryFilterType.BLOCKED_FROM_TARGET, false )],
				[new EEnvQueryScoring( EEnvQueryScoringType.DISTANCE, Owner.GameObject, false )], new Vector3( 0, 0, 20.0f ), new Vector3( 0, 0, 60.0f ) );
			}
			else
			{
				query = new DonutEnvironmentQuery( Owner.GameObject, currentHostile, Owner.WorldPosition, 2000.0f, 0.0f, 110.0f, 15.0f, EEnvQueryAxisType.ONLY_XY, "ai_cover"
				, [new EEnvQueryFilter( EEnvQueryFilterType.COLLISION, true ), new EEnvQueryFilter( EEnvQueryFilterType.BLOCKED_FROM_TARGET, false )],
				[new EEnvQueryScoring( EEnvQueryScoringType.DISTANCE, Owner.GameObject, false ), new EEnvQueryScoring( EEnvQueryScoringType.DISTANCE, currentHostile, false )], new Vector3( 0, 0, 20.0f ), new Vector3( 0, 0, 30.0f ) );
			}

			StartSprinting();
			RunEnvironmentQuery( query, EEnvQueryResultType.BEST, OnFindCoverFinished );
			return Action.Result.Progress;
		}

		if ( Owner.AIEnvironmentQueryHandler.QueryStatus == NPBehave.Action.Result.Failed )
			return Action.Result.Failed;

		return Action.Result.Success;


	}

	private void CrouchBehindCover( )
	{
		Owner.CrouchBehindCover();
	}

	private void StandFromCover()
	{
		Owner.StandUpFromCover();
		StartAimWalking();
	}

	private void OnFindCoverFinished( EnvQueryResult result )
	{
		if ( !result.Success ) return;

		Owner.SetNewTargetLocation( result.Location );
	}


	private Action.Result ShootAction( Action.Request args )
	{
		Owner.Shoot();
		return Action.Result.Success;
	}
}
