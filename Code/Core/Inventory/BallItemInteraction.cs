using Sandbox;

public sealed class BallItemInteraction : SimpleInteractions.SimpleInteraction
{
	public BallItem BallItem { get; private set; }
	[Property] public TempPlayerController TempPlayerController { get; private set; }

	protected override void OnAwake()
	{
		BallItem = new BallItem();
		BallItem.PobxItemReference = this;
	}

	protected override void OnStart()
	{
		base.OnStart();

		// Put your initialization code here if you have any
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// Put your update code here if you have any
	}


	[Rpc.Broadcast]
	protected override void OnInteract()
	{
		this.
		TempPlayerController.AddItem( BallItem );
		//GameObject.Enabled = false;
		Log.Info($"{Rpc.Caller.DisplayName} interacted with {this.GameObject.Name}!");
		//GameObject.Destroy();
	}
}
