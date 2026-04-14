using System.Diagnostics;
using NPBehave;

namespace Sandbox.Samples;

public class NPBehaveExampleHelloBlackboardsAI : Component
{
	
	public Root _behaviorTree {get; set;}

	protected override void OnStart()
	{
		
		_behaviorTree = new Root(

			// toggle the 'toggled' blackboard boolean flag around every 500 milliseconds
			new Service(0.5f, () => { _behaviorTree.Blackboard["foo"] = !_behaviorTree.Blackboard.Get<bool>("foo"); }, 

				new Selector(

					// Check the 'toggled' flag. Stops.IMMEDIATE_RESTART means that the Blackboard will be observed for changes 
					// while this or any lower priority branches are executed. If the value changes, the corresponding branch will be
					// stopped and it will be immediately jump to the branch that now matches the condition.
					new BlackboardCondition("foo", Operator.IsEqual, true, Stops.ImmediateRestart, 

						// when 'toggled' is true, this branch will get executed.
						new Sequence(

							// print out a message ...
							new Action(() => Log.Info("foo")),

							// ... and stay here until the `BlackboardValue`-node stops us because the toggled flag went false.
							new WaitUntilStopped()
						)
					),

					// when 'toggled' is false, we'll eventually land here
					new Sequence(
						new Action(() => Log.Info("bar")),
						new WaitUntilStopped()
					)
				)
			)
		);
		_behaviorTree.Start();
		base.OnStart();
	}

	
}
