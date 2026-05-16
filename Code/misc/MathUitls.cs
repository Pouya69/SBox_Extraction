using Sandbox;

public static class MathUtils
{
	public static Vector3 VInterpTo( Vector3 current, Vector3 target, float deltaTime, float interpSpeed )
	{
		return current + (target - current) * Math.Clamp( interpSpeed * deltaTime, 0f, 1f );
	}

	public static float FInterpTo( float current,float target, float deltaTime, float interpSpeed )
	{
		return current + (target - current) * Math.Clamp( interpSpeed * deltaTime, 0f, 1f );
	}

	public static Vector3 VInterpConstantTo( Vector3 current, Vector3 target, float deltaTime, float speed )
	{
		Vector3 delta = target - current;
		float distance = delta.LengthSquared;

		if ( distance <= 0.0001f * 0.0001f )
			return target;

		float maxStep = speed * deltaTime;

		if ( distance <= maxStep )
			return target;

		return current + delta.Normal * maxStep;
	}

	public static float FInterpConstantTo( float current, float target, float deltaTime, float speed )
	{
		float delta = target - current;
		float deltaAbs = Math.Abs( delta );

		if ( deltaAbs <= 0.0001f )
			return target;

		float maxStep = speed * deltaTime;

		if ( deltaAbs <= maxStep )
			return target;

		return current + delta * maxStep;
	}
}
