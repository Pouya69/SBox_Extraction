using Sandbox;

public sealed class ExtractionQuestEntityComponent : Component, IExtractionQuestEntity
{
	[Property] public QuestEntityInfo EntityInfo;
	[Property] private ActionSystemComponent EntityActionSystemComponent;

	public void AddEntityToGlobalManager()
	{
		throw new System.NotImplementedException();
	}

	public void EntityKilled()
	{
		ExtractionQuestSystem.EntityKilled(this);
	}

	public void EntityPickedUp()
	{
		ExtractionQuestSystem.EntityPickedUp(this);
	}

	public bool IsAlive()
	{
		throw new System.NotImplementedException();
	}
}
