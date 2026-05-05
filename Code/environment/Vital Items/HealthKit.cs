using Sandbox;

public sealed class HealthKit : Component, IInteractable
{
	[Property, Feature("Health Kit")] public float HealAmount { get; private set; }
	public bool CanBePickedUp()
	{
		return false;
	}

	public void Interact( PlayerInteractionComponent interactionComponent )
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

	public bool IsPickUpTwoHanded()
	{
		return false;
	}
}
