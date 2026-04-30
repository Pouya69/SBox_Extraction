using Sandbox;
using static Sandbox.Services.Inventory;

public sealed class PlayerInteractionComponent : Component
{
	[Property, Group( "Config" )] private bool DebugInteraction { get; set; } = false;
	[Property, Group("Config")] private float InteractionAcceptableDistance { get; set; } = 50.0f;
	[Property, Group( "Config" )] private float InteractionFindRadius { get; set; } = 200.0f;
	[Property, Group( "Config" )] private float InteractionFindDistance { get; set; } = 80.0f;
	[Property, RequireComponent, Group( "Components" )] public ExtractionPlayerQuestSystemHandlerComponent PlayerQuestSystem { get; private set; }
	private GameObject FocusedInteractable;

	protected override void OnFixedUpdate()
	{
		FindBestInteractable();
	}

	private void FindBestInteractable()
	{
		Vector3 start = this.WorldPosition;
		Vector3 end = start + this.WorldTransform.Forward * InteractionFindDistance;

		var results = Scene.Trace.Sphere( InteractionFindRadius, start, end ).IgnoreGameObjectHierarchy(this.GameObject).WithTag( "interaction" ).RunAll();

		if ( !results.Any() )
		{
			FocusedInteractable = null;
			return;
		}

		float closestDistance = 99999999.0f;
		float bestDot = -2.0f;

		foreach ( var item in results )
		{

			float dot = (item.Component.WorldPosition - start).Dot( item.Direction );

			if ( DebugInteraction )
			{
				float distance = item.Component.WorldPosition.Distance( start );
				string debugString = item.GameObject.Name + "\tDot: " + dot + "\t" + "Distance: " + distance;
				Log.Info( debugString );
				DebugOverlay.Text( item.Component.WorldPosition, debugString );
			}

			if (dot > bestDot)
			{
				float distance = item.Component.WorldPosition.Distance( start );
				if ( distance < closestDistance )
				{
					FocusedInteractable = item.GameObject;
					closestDistance = distance;
					bestDot = dot;
				}
			}
		}

		if ( DebugInteraction && FocusedInteractable != null)
		{
			DebugOverlay.Sphere( new Sphere( FocusedInteractable.WorldPosition, 50.0f ), Color.Green );
		}
	}

	public void AttemptInteract()
	{
		if ( FocusedInteractable != null )
			FocusedInteractable.Parent.GetComponent<IInteractable>().Interact(this);
	}
}
