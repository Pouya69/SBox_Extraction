using NPBehave;
using Sandbox;
using System;
using System.Collections;
using System.Threading.Tasks;
using Random = System.Random;
using Task = System.Threading.Tasks.Task;

public sealed class EnvironmentQueryHandler : Component
{
	[Group("Debug")] [Property] private bool DebugQueries { get; set; } = false;
	[Group( "Debug" )] [Property] private float DebugLastDuration { get; set; } = 5.0f;
	[Group( "Debug" )] [Property] public GameObject DebugPlayerRef { get; private set; }
	[Property] private int AwaitEveryItems { get; set; } = 3;

	public event Action<EnvQueryResult> OnQueryCompleted;

	private List<EnvQueryPoint> PotentionalGoodPoints = new();

	private Action<EnvQueryResult> CurrentFunctor;

	public NPBehave.Action.Result QueryStatus { get; private set; }

	//private TimeSince timeSinceEQS;

	[Button("Debug Find Cover (Donut)")]
	private void DebugFindCover()
	{

		DonutEnvironmentQuery query = new DonutEnvironmentQuery( GameObject, DebugPlayerRef, WorldPosition, 2000.0f, 50.0f, 100.0f, 15.0f, EEnvQueryAxisType.ONLY_XY, "ai_cover"
			, [new EEnvQueryFilter( EEnvQueryFilterType.COLLISION, true), new EEnvQueryFilter( EEnvQueryFilterType.BLOCKED_FROM_TARGET, false )],
			[new EEnvQueryScoring( EEnvQueryScoringType.DISTANCE, GameObject, false )], new Vector3( 0, 0, 20.0f ), new Vector3( 0, 0, 60.0f ) );

		RunQuery( query, EEnvQueryResultType.BEST, DebugOnQueryCompleted);
	}

	private void DebugOnQueryCompleted(EnvQueryResult result)
	{

		
	}

	public async void RunQuery( DonutEnvironmentQuery query, EEnvQueryResultType resultType, Action<EnvQueryResult> onQueryCompletedFunctor )
	{
		QueryStatus = NPBehave.Action.Result.Progress;
		PotentionalGoodPoints.Clear();
		// timeSinceEQS = 0.0f;

		CurrentFunctor = onQueryCompletedFunctor;
		OnQueryCompleted += onQueryCompletedFunctor;

		Vector3 currentLocationForward = query.OwnerOffset + query.StartLocation + Vector3.Forward * query.InnerRadius;
		Vector3 currentLocationBack = query.OwnerOffset + query.StartLocation + Vector3.Backward * query.InnerRadius;
		Vector3 currentLocationRight = query.OwnerOffset + query.StartLocation + Vector3.Right * query.InnerRadius;
		Vector3 currentLocationLeft = query.OwnerOffset + query.StartLocation + Vector3.Left * query.InnerRadius;

		Vector3 currentLocationFR = query.OwnerOffset + query.StartLocation + (new Vector3( 1, 1, 0 ) * query.InnerRadius);
		Vector3 currentLocationFL = query.OwnerOffset + query.StartLocation + (new Vector3( 1, -1, 0 ) * query.InnerRadius);
		Vector3 currentLocationBR = query.OwnerOffset + query.StartLocation + (new Vector3( -1, 1, 0 ) * query.InnerRadius);
		Vector3 currentLocationBL = query.OwnerOffset + query.StartLocation + (new Vector3( -1, -1, 0 ) * query.InnerRadius);

		int amountOfPointsOnEachDirection = (int)((query.Radius - query.InnerRadius) / query.PointDistance) + 1;

		// Filtering
		for ( int i = 0; i < amountOfPointsOnEachDirection; i++ )
		{
			currentLocationForward += Vector3.Forward * query.PointDistance;
			currentLocationBack += Vector3.Backward * query.PointDistance;
			currentLocationRight += Vector3.Right * query.PointDistance;
			currentLocationLeft += Vector3.Left * query.PointDistance;

			currentLocationFR += new Vector3( 1, 1, 0 ) * query.PointDistance;
			currentLocationFL += new Vector3( 1, -1, 0 ) * query.PointDistance;
			currentLocationBR += new Vector3( -1, 1, 0 ) * query.PointDistance;
			currentLocationBL += new Vector3( -1, -1, 0 ) * query.PointDistance;



			await FilterResults( query, [currentLocationForward, currentLocationBack, currentLocationRight, currentLocationLeft, currentLocationFR, currentLocationFL, currentLocationBR, currentLocationBL] );


			if (i % AwaitEveryItems == 0)
			{
				await Task.Frame();
				//Log.Info( "Time taken: " + timeSinceEQS + " seconds" );
				//timeSinceEQS = 0.0f;
			}
				
		}

		// Scoring
		await FindBestFromScore(query);

		var result = CalculateResult( resultType);




		QueryCompleted( ref result );
	}

	private EnvQueryResult CalculateResult( EEnvQueryResultType resultType)
	{
		switch ( resultType )
		{
			case EEnvQueryResultType.BEST:
				return GetBestResult();

			case EEnvQueryResultType.BEST_25_PERCENT:
				return GetBest25PercentResult();

			case EEnvQueryResultType.RANDOM:
				return GetRandomResult();

			case EEnvQueryResultType.BEST_10_PERCENT:
				return GetBest10PercentResult();

			case EEnvQueryResultType.BEST_5_PERCENT:
				return GetBest5PercentResult();

			default:
				return GetBestResult();
		}
	}

	private EnvQueryResult GetBestResult()
	{
		EnvQueryResult OutQueryResult = new();

		OutQueryResult.Success = PotentionalGoodPoints.Count > 0;
		if ( OutQueryResult.Success )
		{
			OutQueryResult.Location = PotentionalGoodPoints[0].Location;
			/*if (DebugQueries)
			{
				for ( int i = 0; i < PotentionalGoodPoints.Count; i++ )
				{
					Log.Warning( (i + 1) + ": " + "Score: " + PotentionalGoodPoints[i].Score + ", Location " + PotentionalGoodPoints[i].Location );
				}
			}*/
			
		}

		return OutQueryResult;
	}

	private EnvQueryResult GetBest25PercentResult()
	{
		EnvQueryResult OutQueryResult = new();

		OutQueryResult.Success = PotentionalGoodPoints.Count > 0;
		if ( OutQueryResult.Success )
		{
			var rand = new Random();
			OutQueryResult.Location = PotentionalGoodPoints[rand.Next( 0, PotentionalGoodPoints.Count / 4 )].Location;
		}

		return OutQueryResult;
	}

	private EnvQueryResult GetBest10PercentResult()
	{
		EnvQueryResult OutQueryResult = new();

		OutQueryResult.Success = PotentionalGoodPoints.Count > 0;
		if ( OutQueryResult.Success )
		{
			var rand = new Random();
			OutQueryResult.Location = PotentionalGoodPoints[rand.Next( 0, PotentionalGoodPoints.Count / 10 )].Location;
		}

		return OutQueryResult;
	}

	private EnvQueryResult GetBest5PercentResult()
	{
		EnvQueryResult OutQueryResult = new();

		OutQueryResult.Success = PotentionalGoodPoints.Count > 0;
		if ( OutQueryResult.Success )
		{
			var rand = new Random();
			OutQueryResult.Location = PotentionalGoodPoints[rand.Next( 0, PotentionalGoodPoints.Count / 20 )].Location;
		}

		return OutQueryResult;
	}

	private EnvQueryResult GetRandomResult()
	{
		EnvQueryResult OutQueryResult = new();

		OutQueryResult.Success = PotentionalGoodPoints.Count > 0;
		if ( OutQueryResult.Success )
		{
			var rand = new Random();
			OutQueryResult.Location = PotentionalGoodPoints[rand.Next( 0, PotentionalGoodPoints.Count)].Location;
		}

		return OutQueryResult;
	}

	private async Task FilterResults(DonutEnvironmentQuery query, Vector3[] locsToCheck )
	{
		if ( query.FilterTypes.Length == 0)
		{
			NoFilter( locsToCheck );
			return;
		}
		List<Vector3> PassedLocs = locsToCheck.ToList();
		foreach ( var filter in query.FilterTypes )
		{
			switch ( filter.FilterType )
			{

				case EEnvQueryFilterType.COLLISION:
					CollisionFilter( ref query, ref PassedLocs, filter.ReversedResult );
					break;

				case EEnvQueryFilterType.DOT:
					DotFilter( ref query, ref PassedLocs, filter.ReversedResult );
					break;

				case EEnvQueryFilterType.BLOCKED_FROM_OWNER:
					BlockedFromFilter( ref query, ref PassedLocs, query.Owner, filter.ReversedResult );
					break;

				case EEnvQueryFilterType.BLOCKED_FROM_TARGET:
					BlockedFromFilter( ref query, ref PassedLocs, query.Target, filter.ReversedResult );
					break;
			}

			await Task.Frame();
		}

		foreach ( var item in PassedLocs )
		{
			PotentionalGoodPoints.Add( new EnvQueryPoint( item, 1.0f ) );
			if ( DebugQueries )
				DebugOverlay.Sphere( new Sphere( item, query.PointRadius ), Color.Green, DebugLastDuration );
		}
		
		
	}

	private void NoFilter( Vector3[] locsToCheck )
	{
		foreach ( var loc in locsToCheck )
			PotentionalGoodPoints.Add( new( loc, 1.0f ) );
	}

	private void BlockedFromFilter( ref DonutEnvironmentQuery query, ref List<Vector3> locsToCheck, GameObject Target, bool reversed)
	{
		for ( int i = locsToCheck.Count - 1; i >= 0; i-- )
		{
			var loc = locsToCheck[i];
			bool traceHit = Scene.Trace.Ray(loc, Target.WorldPosition + query.TargetOffset ).IgnoreGameObjectHierarchy( query.Target ).IgnoreGameObjectHierarchy(query.Owner).Run().Hit;
			if ( reversed )
				traceHit = traceHit ? false : true;

			if ( traceHit )
			{
				//if ( DebugQueries )
				//DebugOverlay.Sphere( new Sphere( loc, query.PointRadius ), Color.Green, DebugLastDuration );
			}
			else
			{
				locsToCheck.RemoveAt( i );
				if ( DebugQueries )
					DebugOverlay.Sphere( new Sphere( loc, query.PointRadius ), Color.Red, DebugLastDuration );
			}
		}
	}

	private void CollisionFilter ( ref DonutEnvironmentQuery query, ref List<Vector3> locsToCheck, bool reversed )
	{
		for ( int i = locsToCheck.Count - 1; i >= 0; i-- ) {
			var loc = locsToCheck[i];

			bool traceHit = Scene.Trace.Sphere( query.PointRadius, loc, loc ).IgnoreGameObjectHierarchy( GameObject ).Run().Hit;
			if ( reversed )
				traceHit = traceHit ? false : true;

			if ( traceHit )
			{
				//if ( DebugQueries )
				//DebugOverlay.Sphere( new Sphere( loc, query.PointRadius ), Color.Green, DebugLastDuration );
			}
			else
			{
				locsToCheck.RemoveAt( i );
				if ( DebugQueries )
					DebugOverlay.Sphere( new Sphere( loc, query.PointRadius ), Color.Red, DebugLastDuration );
			}
		}
	}

	private void DotFilter( ref DonutEnvironmentQuery query, ref List<Vector3> locsToCheck, bool reversed, float DotGreaterThan = 0.0f )
	{
		Vector3 ownerForward = query.Owner.WorldTransform.Forward;
		Vector3 ownerLocation = query.StartLocation;

		for ( int i = locsToCheck.Count - 1; i >= 0; i-- )
		{
			var loc = locsToCheck[i];
			bool condition = ownerForward.Dot( (loc - query.StartLocation) ) >= DotGreaterThan;
			if (reversed)
				condition = condition ? false : true;

			if ( condition )
			{
				//if ( DebugQueries )
				//DebugOverlay.Sphere( new Sphere( loc, query.PointRadius ), Color.Green, DebugLastDuration );
			}
			else
			{
				locsToCheck.RemoveAt( i );
				if ( DebugQueries )
					DebugOverlay.Sphere( new Sphere( loc, query.PointRadius ), Color.Red, DebugLastDuration );
			}

		}

	}

	private async Task FindBestFromScore(DonutEnvironmentQuery query)
	{

		foreach ( var scoring in query.Scorings )
		{
			switch ( scoring.ScoringType )
			{
				case EEnvQueryScoringType.NO_SCORING:
					return;

				default:
					return;

				case EEnvQueryScoringType.DISTANCE:
					await DistanceScoreMethod( query, scoring );
					return;
			}
		}
	}

	private async Task DistanceScoreMethod(DonutEnvironmentQuery query, EEnvQueryScoring scoring )
	{
		if ( PotentionalGoodPoints.Count == 0 ) return;

		var relativeToPosition = scoring.RelativeTo.WorldPosition;

		for ( int i = 0; i < PotentionalGoodPoints.Count; i++ )
		{
			var item = PotentionalGoodPoints[i];
			if ( scoring.ReversedResult )
				item.Score += relativeToPosition.DistanceSquared( item.Location ) / (query.Radius * query.Radius);
			else
				item.Score += 1.0f - ((relativeToPosition.DistanceSquared( item.Location ) / (query.Radius * query.Radius)));

			PotentionalGoodPoints[i] = item;
			if ( i % AwaitEveryItems == 0 )
			{
				await Task.Frame();
				//
				//timeSinceEQS = 0.0f;
			}
				
		}
		await Task.Frame();
		PotentionalGoodPoints.Sort( ( x, y ) => y.Score.CompareTo( x.Score ) );

		if (DebugQueries)
		{
			for ( int i = 0; i < PotentionalGoodPoints.Count; i++ )
				Log.Info( i + ": " + PotentionalGoodPoints[i].Score );
		}
	}
		

	private void QueryCompleted(ref EnvQueryResult result)
	{
		PotentionalGoodPoints.Clear();
		OnQueryCompleted?.Invoke( result );
		OnQueryCompleted -= CurrentFunctor;
		CurrentFunctor = null;
		QueryStatus = result.Success ? NPBehave.Action.Result.Success : NPBehave.Action.Result.Failed;

		if (DebugQueries)
		{
			if ( result.Success )
			{
				DebugOverlay.Sphere( new Sphere( result.Location, 50.0f ), Color.Blue, 10.0f );

				Log.Info( "EQS WORKS... Result: " + result.Location + ", Request Location: " + GameObject.WorldPosition );
			}
			else
				Log.Info( "EQS Failed..." );
		}	}
}
