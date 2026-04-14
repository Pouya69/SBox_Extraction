using NPBehave;
using Sandbox;

public sealed class SimpleBehavior : Component
{
	public Root _behaviorTree { get; set; }
	
	private Blackboard _blackboard; 
	private Clock _clock;
	
	protected override void OnStart()
	{
		_clock = new Clock();
		_blackboard = new Blackboard(_clock);
		
		_blackboard.Set( "MyVector3", new Vector3( 8 ));
		_blackboard.Set( "MyGameObject", GameObject);
		_blackboard.Set( "ASimpleBool", true);
		
		
		_behaviorTree = new Root(_blackboard, 
			new Selector(
		new Cooldown( 3f, false, false, true, 
						new Action(
							() =>
							{
								_blackboard.Set( "ASimpleBool", !_blackboard.Get<bool>( "ASimpleBool" ) );
							}) {Label = "3 sec Countdown"}),
					new BlackboardCondition( "ASimpleBool", Operator.IsEqual, true, Stops.ImmediateRestart, new Action( () => { Log.Info( "ASimpleBool is true" ); } ) {Label = "ASimpleBool is true"} )
					//new WaitUntilStopped( )
			));
		_behaviorTree.Start();
		
		
	}

	protected override void OnUpdate()
	{
		_clock.Update( Time.Delta );
		base.OnUpdate();
	}
}
