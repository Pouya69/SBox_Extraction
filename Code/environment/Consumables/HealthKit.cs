using Sandbox;

public sealed class HealthKit : ConsumableBase
{
	[Property, Feature("Health Kit")] public float HealAmount { get; private set; }

	public override void Interact( PlayerInteractionComponent interactionComponent )
	{
		if ( interactionComponent.GameObject.GetComponent<ActionSystemComponent>( true ) is var actionSystemComp )
		{
			if ( actionSystemComp.CanBeHealed() )
			{
				actionSystemComp.Heal( this.GameObject, HealAmount );
				DestroyGameObject();
			}
		}
		
	}
}
