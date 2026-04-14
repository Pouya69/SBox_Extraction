using Sandbox;

public sealed class WaterPond : Component
{
	[Property] private Collider WaterCollider;

	protected override void OnStart()
	{
		WaterCollider.OnTriggerEnter += OnObjectEnteredWater;
	}

	private void OnObjectEnteredWater( Collider obj )
	{
		
	}
}
