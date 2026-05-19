using Conna.Inventory;
using Sandbox;

public sealed class PlayerInteractionComponent : Component, IInteractionComp
{
	[Property, Group( "Config" )] private bool DebugInteraction { get; set; } = false;
	[Property, Group("Config")] private float InteractionAcceptableDistance { get; set; } = 50.0f;
	[Property, Group( "Config" )] private float InteractionFindRadius { get; set; } = 200.0f;
	[Property, Group( "Config" )] private float InteractionFindDistance { get; set; } = 80.0f;
	[Property, Group( "Hold" )] private float IK_HoldScanRadius = 6.0f;
	[Property, Group( "Hold" )] private float IK_HoldMaxHandsDistance = 8.0f;
	[Property, Group( "Hold" )] private GameObject IK_HoldPositionReference;
	[Property, Group( "Hold" )] private float DroppingLaunchStrength = 500.0f;
	[Property, Group( "Hold" )] public bool Hold_IK_Enabled { get; private set; }

	[Property, RequireComponent, Group( "Components" )] public ExtractionPlayerQuestSystemHandlerComponent PlayerQuestSystem { get; private set; }
	[Property, RequireComponent, Group("Components")] public PobxPlayer Player {  get; private set; }
	private GameObject FocusedInteractable;

	private IInteractable _interactingInteractable;

	// Temporary For Now.
	public IExtractionQuestEntity GrabbingEntity { get; private set; }
	public bool IsHoldingObject { get; private set; }

	protected override void OnFixedUpdate()
	{
		FindBestInteractable();

		// Out of range.
		if ( FocusedInteractable == null && _interactingInteractable != null )
			Released();

		//if ( Hold_IK_Enabled && IsHoldingObject )
		//SetHandPositionAroundGrabbedObject_IK();
	}

	private void FindBestInteractable()
	{
		Vector3 start = this.Player.EyeTransform.Position;
		Vector3 end = start + this.Player.EyeTransform.Forward * InteractionFindDistance;

		var results = Scene.Trace.Sphere( InteractionFindRadius, start, end ).IgnoreGameObjectHierarchy(this.Player.GameObject).WithTag( "interaction" ).UseHitPosition(true).RunAll();

		if ( !results.Any() )
		{
			FocusedInteractable = null;
			return;
		}

		float closestDistance = 99999999.0f;
		float bestDot = -200000.0f;

		GameObject BestInteractableSoFar = null;
		foreach ( var item in results )
		{
			if ( !item.Hit ) continue;

			float dot = (item.HitPosition - start).Normal.Dot( item.Direction );
			if ( dot < 0 ) continue;

			var blockedByObstacleTrace = Scene.Trace.Ray( start, item.HitPosition ).IgnoreGameObjectHierarchy( this.Player.GameObject ).WithTag("solid").Run();
			if ( blockedByObstacleTrace.Hit && !blockedByObstacleTrace.GameObject.Equals(item.GameObject) )
			{
				if (DebugInteraction)
				{
					DebugOverlay.Sphere(new Sphere( item.HitPosition, 2.0f), Color.Orange);
					Log.Info( $"{item.GameObject.Name} is blocked by {blockedByObstacleTrace.GameObject.Name}" );
				}
				continue;
			}

			BestInteractableSoFar ??= item.GameObject;

			if ( DebugInteraction )
			{
				float distancea = item.HitPosition.Distance( start );
				string debugString = item.GameObject.Name + "\nDot: " + dot + "\n" + "Distance: " + distancea;
				Log.Info( debugString );
				DebugOverlay.Text( item.HitPosition, debugString );
			}

			float distance = item.HitPosition.Distance( start );
			if ( dot > bestDot && distance <= closestDistance )
			{
				bestDot = dot;
				closestDistance = distance;
				BestInteractableSoFar = item.GameObject;
			}
		}
		FocusedInteractable = BestInteractableSoFar;

		if ( DebugInteraction && FocusedInteractable != null)
		{
			DebugOverlay.Sphere( new Sphere( FocusedInteractable.WorldPosition, 50.0f ), Color.Green );
		}
	}

	public void AttemptInteract()
	{
		if ( FocusedInteractable != null )
		{
			IInteractable interactable = FocusedInteractable.GetComponent<IInteractable>();
			if ( interactable == null)
				interactable = FocusedInteractable.Parent.GetComponent<IInteractable>(); ;

			if ( interactable == null )
				return;

			if (interactable.IsInteractable())
			{
				interactable.Interact( this );
				_interactingInteractable = interactable;
			}
			
		}
			
	}

	public void Released()
	{
		if (_interactingInteractable == null)
		{
			return;
		}

		_interactingInteractable.Released();
		_interactingInteractable = null;
	}

	public async void PickUpEntity(IExtractionQuestEntity entity) {
		if (IsHoldingObject)
		{
			DropHeldEntity();
		}

		entity.GetCollider().Enabled = false;
		await Task.Frame();
		entity.GetGameObject().SetParent( IK_HoldPositionReference, true );
		entity.GetGameObject().LocalPosition = Vector3.Zero;
		entity.GetGameObject().LocalRotation = Rotation.Identity;

		GrabbingEntity = entity;
		IsHoldingObject = true;

		// Player.PickupObjectTwoHandedAnimation();
	}

	public async void DropHeldEntity()
	{
		if ( IsHoldingObject )
		{
			GrabbingEntity.GetGameObject().SetParent( null );
			var launchVel = DroppingLaunchStrength * IK_HoldPositionReference.WorldTransform.Forward;
			GrabbingEntity.LaunchEntity( launchVel );
			// Player.DropGrabbedEntityAnimation();
			IsHoldingObject = false;

			await Task.Frame();

			GrabbingEntity.GetCollider().Enabled = true;
			GrabbingEntity = null;
		}
	}

	/*
	public void SetHandPositionAroundGrabbedObject_IK()
	{
		Vector3 leftHandForward = Player.LeftHandSocket.WorldTransform.Forward;
		Vector3 rightHandForward = Player.RightHandSocket.WorldTransform.Forward;

		Vector3 holdPointMiddle = IK_HoldPositionReference.WorldPosition;
		Vector3 maxHoldPointLeft = holdPointMiddle - IK_HoldMaxHandsDistance * Transform.World.Right;
		Vector3 maxHoldPointRight = holdPointMiddle + IK_HoldMaxHandsDistance * Transform.World.Right;


		var leftHandScanResult = Scene.Trace.Sphere( IK_HoldScanRadius, maxHoldPointLeft, holdPointMiddle ).WithTag("interaction").UseHitPosition(true).Run();
		var rightHandScanResult = Scene.Trace.Sphere( IK_HoldScanRadius, maxHoldPointRight, holdPointMiddle ).WithTag( "interaction" ).UseHitPosition( true ).Run();

		Vector3 finalLeftPos = Player.LeftHandSocket.WorldPosition;
		Rotation finalLeftRot = Player.LeftHandSocket.WorldRotation;
		Vector3 finalRightPos = Player.RightHandSocket.WorldPosition;
		Rotation finalRightRot = Player.RightHandSocket.WorldRotation;

		if (leftHandScanResult.Hit)
		{
			finalLeftPos = leftHandScanResult.HitPosition;
			finalLeftRot = Rotation.LookAt( new Vector3( 0, 0, 0), leftHandScanResult.Normal );
		}

		if ( rightHandScanResult.Hit ) {
			finalRightPos = rightHandScanResult.HitPosition;
			finalRightRot = Rotation.LookAt( Vector3.Zero, rightHandScanResult.Normal );
		}

		Player.IK_SetHandsHoldingPositionsAndRotations(finalLeftPos, finalLeftRot, finalRightPos, finalRightRot);
	}
	*/

	public void OnControl()
	{
		if ( Input.Pressed( "Use" ) )
		{
			AttemptInteract();
		}
		else if (Input.Released("Use"))
		{
			Released();
		}
	}

	public InventoryResult AddItemToInventory( PobxBaseInventoryItem item ) {
		return Player.InventoryComponent.AddItemToInventory( item );
	}

	public GameObject GetGameObject() => this.Player.GameObject;

	public IExtractionQuestEntity GetEntity() => Player.EntityComponent;

	public GameObject GetAttachmentGameObject() => Player.PlayerState.GameObject;
}
