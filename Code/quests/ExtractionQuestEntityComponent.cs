using Sandbox;

public class ExtractionQuestEntityComponent : Component, IExtractionQuestEntity
{
	[Property, RequireComponent] protected Rigidbody rigidbody { get; set; }
	[Property] protected Collider collider { get; set; }
	[Property] public QuestEntityInfo EntityInfo;
	[Property] protected ActionSystemComponent EntityActionSystemComponent;
	[Property] protected EExtractionObjectSize ObjectSize { get; set; } = EExtractionObjectSize.SMALL;
	[Property, RequireComponent] protected Renderer renderer { get; set; }
	/// <summary>
	/// Should this entity cast events to the quest system? (e.g. killed, entered, picked up)
	/// If not, it will only be considered for logics.
	/// </summary>
	[Property] public bool ShouldReportToQuestSystem { get; protected set; } = true;

	public void AddEntityToGlobalManager()
	{
		throw new System.NotImplementedException();
	}

	public void EnteredArea(QuestLocationInfo locationInfo)
	{
		if ( ShouldReportToQuestSystem )
			ExtractionQuestSystem.LocationEntered(this, locationInfo);
	}

	public void EntityKilled( IExtractionQuestEntity Instigator )
	{
		if ( ShouldReportToQuestSystem )
			ExtractionQuestSystem.EntityKilled(Instigator, this);
	}

	public void EntityPickedUp( IExtractionQuestEntity Instigator )
	{
		if ( ShouldReportToQuestSystem )
			ExtractionQuestSystem.EntityPickedUp( Instigator, this );
	}

	public string GetEntityName()
	{
		return EntityInfo.EntityName;
	}

	public bool IsAlive()
	{
		return EntityActionSystemComponent.IsAlive();
	}

	public virtual void LaunchEntity( Vector3 velocity, bool ignoreMass = true )
	{
		if ( rigidbody == null ) return;

		rigidbody.Sleeping = false;
		rigidbody.Velocity = ignoreMass ? velocity : (rigidbody.Velocity + velocity);
		// rigidbody.ApplyImpulse( velocity );
		// Log.Info( rigidbody.Velocity );
	}

	public EExtractionObjectSize GetObjectSize()
	{
		return ObjectSize;
	}

	public Renderer GetRenderer()
	{
		return renderer;
	}

	public virtual void ToggleEnablePhysics( bool enable )
	{
		rigidbody.MotionEnabled = enable;
	}

	public GameObject GetGameObject()
	{
		return GameObject;
	}

	public virtual bool CanBeRemoteGrabbed()
	{
		return ObjectSize == EExtractionObjectSize.SMALL || ObjectSize == EExtractionObjectSize.CREATURE_TYPE;
	}

	public Rigidbody GetRigidbody()
	{
		return rigidbody;
	}

	public Collider GetCollider()
	{
		return collider;
	}
}
