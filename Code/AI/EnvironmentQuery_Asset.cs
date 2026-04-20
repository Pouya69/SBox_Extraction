

public enum EEnvQueryAxisType
{
	ALL_AXIS,
	ONLY_XY,
}

public enum EEnvQueryResultType
{
	BEST,
	RANDOM,
	BEST_25_PERCENT,
	BEST_10_PERCENT,
	BEST_5_PERCENT
}

public struct EnvQueryPoint
{
	public Vector3 Location;
	public float Score;

	public EnvQueryPoint( Vector3 location , float score) {
		Location = location;
		Score = score;
	}
}

public class EnvironmentQuery
{
	public EEnvQueryAxisType AxisType;
	public GameObject Owner;
	public GameObject Target;  // For blocked filtering.
	public Vector3 OwnerOffset;
	public Vector3 TargetOffset;
	public Vector3 StartLocation;
	public float PointDistance;
	public float PointRadius;
	public string IgnoreTag;
	public EEnvQueryFilter[] FilterTypes;
	public EEnvQueryScoring[] Scorings;

	public EnvironmentQuery(GameObject owner, GameObject target, Vector3 startLocation, float pointDistance, float pointRadius, EEnvQueryAxisType axisType, string ignoreTag, EEnvQueryFilter[] filterTypes, EEnvQueryScoring[] scorings,
		Vector3 ownerOffset = new Vector3(), Vector3 targetOffset = new Vector3() ) {
		Owner = owner;
		Target = target;
		StartLocation = startLocation;
		PointDistance = pointDistance;
		PointRadius = pointRadius;
		AxisType = axisType;
		IgnoreTag = ignoreTag;
		FilterTypes = filterTypes;
		Scorings = scorings;
		TargetOffset = targetOffset;
		OwnerOffset = ownerOffset;
	}

}

public struct EnvQueryResult
{
	public bool Success;
	public Vector3 Location;

	public EnvQueryResult( bool success, Vector3 location ) { 
		Success = success;
		Location = location;
	}

	public EnvQueryResult() { }
}

public struct EEnvQueryFilter
{
	public EEnvQueryFilterType FilterType;
	public bool ReversedResult;

	public EEnvQueryFilter( EEnvQueryFilterType filterType, bool reversedResult)
	{
		FilterType = filterType;
		ReversedResult= reversedResult;
	}
}

public struct EEnvQueryScoring
{
	public EEnvQueryScoringType ScoringType;
	public GameObject RelativeTo;
	public bool ReversedResult;
	public EEnvQueryScoringFallOffType FallOffType = EEnvQueryScoringFallOffType.QUADRATIC;

	public EEnvQueryScoring( EEnvQueryScoringType scoringType, GameObject relativeTo, bool reversedResult )
	{
		ScoringType = scoringType;
		ReversedResult = reversedResult;
		RelativeTo = relativeTo;
	}
}

public enum EEnvQueryFilterType
{
	COLLISION,
	DOT,
	BLOCKED_FROM_OWNER,
	BLOCKED_FROM_TARGET
}

public enum EEnvQueryScoringFallOffType
{
	LINEAR,
	QUADRATIC
}

public enum EEnvQueryScoringType
{
	NO_SCORING,
	DISTANCE
}

public class DonutEnvironmentQuery : EnvironmentQuery
{
	public float Radius;
	public float InnerRadius;
	public DonutEnvironmentQuery(GameObject owner, GameObject target, Vector3 startLocation, float radius, float innerRadius, float pointDistance, float pointRadius, EEnvQueryAxisType axisType, string ignoreTag
		, EEnvQueryFilter[] filterTypes, EEnvQueryScoring[] scorings, Vector3 ownerOffset = new Vector3(), Vector3 targetOffset = new Vector3() )
		: base(owner, target, startLocation, pointDistance, pointRadius, axisType, ignoreTag, filterTypes, scorings, ownerOffset, targetOffset)
	{
		Radius = radius;
		InnerRadius = innerRadius;
	}

	
}
