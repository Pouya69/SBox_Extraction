using Sandbox;

public sealed class JumperPadComponent : Component
{
	/// <summary>
	/// Uses the up vector of the object.
	/// </summary>
	[Property] private float LaunchVelocity = 500.0f;
	[Property] private Vector3 LaunchVelocityAddition = new(0,0,50.0f);
	[Property, RequireComponent] private BoxCollider collider { get; set; }

	protected override void OnAwake()
	{
		collider.OnObjectTriggerEnter += OnPhysicsObjectEnteredJumpPad;
	}

	protected override void OnDestroy()
	{
		collider.OnObjectTriggerEnter -= OnPhysicsObjectEnteredJumpPad;
	}

	private void OnPhysicsObjectEnteredJumpPad(GameObject gameObject)
	{
		var entityComponent = gameObject.GetComponent<IExtractionQuestEntity>();
		if ( entityComponent == null)
			return;

		entityComponent.LaunchEntity( (LaunchVelocity * Transform.World.Up) + LaunchVelocityAddition );
	}
}
