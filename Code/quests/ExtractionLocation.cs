using Sandbox;

public sealed class ExtractionLocation : Component
{
	[Property] public QuestLocationInfo LocationInfo { get; private set; }
	[Property, RequireComponent] private SphereCollider LocationCollisionTrigger { get; set; }

	protected override void OnAwake()
	{
		if ( LocationCollisionTrigger == null )
			LocationCollisionTrigger = GetComponent<SphereCollider>();

		LocationCollisionTrigger.OnObjectTriggerEnter += OnEntityEnteredLocation;
	}

	protected override void OnDestroy()
	{
		LocationCollisionTrigger.OnObjectTriggerEnter -= OnEntityEnteredLocation;
	}

	private void OnEntityEnteredLocation( GameObject Entity)
	{
		var entityComponent = Entity.GetComponent<ExtractionQuestEntityComponent>();
		if (entityComponent == null)
		{
			// Log.Error( "Collision setup for object " + Entity.Name + " entered " + this.GameObject.Name + " is invalid. Check both." );
			return;
		}

		entityComponent.EnteredArea(LocationInfo);
	}
}
