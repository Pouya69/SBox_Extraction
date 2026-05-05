using Sandbox.Utility;

namespace Sandbox.CameraNoise;

public class Recoil : BaseCameraNoise
{
	public Recoil(Vector3 amount, float speed = 1)
	{
		new FullShake() { ShakeSize = 0.5f * amount * GamePreferences.ScreenShakeMultiplier, Waves = 3.0f * speed};
	}

	public Recoil( float amount, float speed = 1)
	{
		new RollShake() { Size = 0.5f * amount * GamePreferences.ScreenShakeMultiplier, Waves = 3.0f * speed };
	}

	public override void ModifyCamera( CameraComponent cc )
	{
	}


}

public class RollShake : BaseCameraNoise
{
	public float Size { get; set; } = 3.0f;
	public float Waves { get; set; } = 3.0f;

	public RollShake()
	{
		LifeTime = 0.3f;
	}

	public override void ModifyCamera( CameraComponent cc )
	{
		var delta = Delta;
		var s = MathF.Sin(delta * MathF.PI * Waves * 2);
		cc.WorldRotation *= new Angles(0, 0, s * Size) * Easing.EaseOut(DeltaInverse);
	}

}

public class FullShake : BaseCameraNoise
{
	public Vector3 ShakeSize { get; set; }
	public float Waves { get; set; } = 3.0f;

	public FullShake()
	{
		LifeTime = 0.3f;
	}

	public override void ModifyCamera( CameraComponent cc )
	{
		var delta = Delta;
		var s = MathF.Sin( delta * MathF.PI * Waves * 2 );
		cc.WorldRotation *= (Rotation.From( (s * ShakeSize).EulerAngles ) * Easing.EaseOut( DeltaInverse ));
	}

}
