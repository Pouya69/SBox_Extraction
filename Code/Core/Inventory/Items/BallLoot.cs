using Sandbox;

public sealed class BallLoot : Component
{

	public BallItem BallItem { get; private set; }

	[Property] private Collider collider { get; set; }

	protected override void OnAwake()
	{
		BallItem = new BallItem();
	}

	protected override void OnStart()
	{
		collider.OnObjectTriggerEnter += OnObjectEnteredTrigger;
	}
	private void OnObjectEnteredTrigger( GameObject player )
	{
		
		var controller = player.GetComponent<TempPlayerController>();
		//Log.Info( controller.Id.ToString() );
		if ( controller != null )
		{
			controller.AddItem( BallItem );
			GameObject.Enabled = false;
		}
		else
		{
			Log.Info( $"no ball" );
		}
	}
}
