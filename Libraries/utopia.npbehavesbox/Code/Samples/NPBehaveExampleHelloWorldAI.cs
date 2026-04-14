using NPBehave;

namespace Sandbox.Samples;

public class NPBehaveExampleHelloWorldAI : Component
{
	private Root _behaviorTree;

	void Start()
	{
		_behaviorTree = new Root(
			new Sequence(
				new Action(() => Log.Info("Hello, World!"))
			)
		);
		_behaviorTree.Start();
	}
}
