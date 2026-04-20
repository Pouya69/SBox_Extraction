using NPBehave;
using Sandbox;

public sealed class TestOnly_AITargetStateTester : Component
{
	private enum ETestOnlyAITargetState
	{
		PATROL,
		HOSTILE,
		NORMAL
	}

	[Property] private Collider collider { get; set; }
	[Property] private ETestOnlyAITargetState StateToSetOnCollide { get; set; }
	[Property] private MyAttemptAI TargetAI { get; set; }
	protected override void OnUpdate()
	{

	}

	protected override void OnStart()
	{
		collider.OnObjectTriggerEnter += OnObjectEnteredTrigger;
	}

	private void OnObjectEnteredTrigger(GameObject gameObject)
	{
		switch (StateToSetOnCollide)
		{
			default:
				return;

			case ETestOnlyAITargetState.HOSTILE:
				TargetAI.DetectedHostile( gameObject );
				break;

			case ETestOnlyAITargetState.NORMAL:
				break;

			case ETestOnlyAITargetState.PATROL:
				TargetAI.EndHostile();
				break;
		}
	}
}
