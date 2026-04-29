using Sandbox;

public sealed class ExtractionQuestEntityComponent : Component, IExtractionQuestEntity
{
	[Property] public QuestEntityInfo EntityInfo;
	[Property] private ActionSystemComponent EntityActionSystemComponent;

	public void AddEntityToGlobalManager()
	{
		throw new System.NotImplementedException();
	}

	public void EnteredArea(QuestLocationInfo locationInfo)
	{
		ExtractionQuestSystem.LocationEntered(this, locationInfo);
	}

	public void EntityKilled( IExtractionQuestEntity Instigator )
	{
		ExtractionQuestSystem.EntityKilled(Instigator, this);
	}

	public void EntityPickedUp( IExtractionQuestEntity Instigator )
	{
		ExtractionQuestSystem.EntityPickedUp( Instigator, this );
	}

	public bool IsAlive()
	{
		return EntityActionSystemComponent.IsAlive();
	}
}
