using Sandbox;

public sealed class JumperPadComponent : Component, Component.ITriggerListener
{
	/// <summary>
	/// Uses the up vector of the object.
	/// </summary>
	[Property] private float LaunchVelocity = 500.0f;
	[Property] private Vector3 LaunchVelocityAddition = new(0,0,50.0f);

	void ITriggerListener.OnTriggerEnter( GameObject other )
	{
		var entityComponent = other.GetComponentInChildren<IExtractionQuestEntity>(true);
		if ( entityComponent == null)
		{
			return;
		}

		entityComponent.LaunchEntity( (LaunchVelocity * Transform.World.Up) + LaunchVelocityAddition );
	}
	
}
