using Sandbox;

public sealed class Credits_Pickable : ConsumableBase
{
	[Property, Feature( "Credits" )] public int Amount { get; private set; }

	public override void Interact( IInteractionComp interactionComponent )
	{
		if ( interactionComponent.GetGameObject().GetComponent<PobxPlayer>(true) is var player && player.IsValid() )
		{
			player.PlayerState.AddCredits( Amount );
			DestroyGameObject();
		}

	}

	protected override void OnUpdate()
	{

	}
}
