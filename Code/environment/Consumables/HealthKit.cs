using Sandbox;

public sealed class HealthKit : ConsumableBase
{
	[Property, Feature("Health Kit")] public float HealAmount { get; private set; }

	public override void Interact( IInteractionComp interactionComponent )
	{
		if ( interactionComponent.GetGameObject().GetComponent<ActionSystemComponent>( true ) is var actionSystemComp )
		{
			if ( actionSystemComp.CanBeHealed() )
			{
				actionSystemComp.Heal( this.GameObject, HealAmount );
				DestroyGameObject();
			}
		}
		
	}
}
