using Sandbox;

public sealed class AI_SightComponent : Component, Component.ITriggerListener
{
	/// <summary>
	/// For ignoring self.
	/// </summary>
	[Property] private GameObject ColliderOfThisNPC { get; set; }
	[Property] private GameObject EyeTransform { get; set; }
	[Property] private bool DebugSightSystem { get; set; } = false;

	/// <summary>
	/// How much up and down cast should be far from the game object?
	/// First is up, second is down
	/// </summary>
	[Property, Group( "Cast" )] private Vector2 EachObjectVerticalCastRange { get; set; } = new( 30, 30 );

	/// <summary>
	/// How much left and right cast should be far from the game object?
	/// First is left, second is right
	/// </summary>
	[Property, Group( "Cast" )] private Vector2 EachObjectHorizentalCastRange { get; set; } = new( 15, 15 );

	/// <summary>
	/// For extra line trace just in case things are moving too fast.
	/// </summary>
	[Property, Group( "Cast" )] private float EachObjectTraceAddition { get; set; } = 20.0f;

	[Property, Group( "Config" )] private HullCollider collider;
	[Property, Group( "Config" )] private float CheckInterval = 0.5f;

	/// <summary>
	/// After Nth item in the for loop, we will check the rest next frame
	/// </summary>
	[Property, Group( "Config" )] private int AsyncNthObjectNextFrame = 7;

	private bool IsAlreadyChecking = false;

	private TimeUntil TimeUntilAISenseCheck;
	public List<GameObject> ObjectsInCollider { get; private set; } = new();
	public List<GameObject> ObjectsInSight { get; private set; } = new();

	public event Action<GameObject> OnSensedObjectInSight;

	/// <summary>
	/// Hiding, behind the cover etc.
	/// </summary>
	public event Action<GameObject> OnSensedObjectOutOfSight;

	protected override void OnUpdate()
	{
		if ( TimeUntilAISenseCheck && !IsAlreadyChecking )
		{
			TimeUntilAISenseCheck = CheckInterval;
			CheckOverlapsInSight();
		}
	}

	private async void CheckOverlapsInSight()
	{
		IsAlreadyChecking = true;
		for ( int i = 0; i < ObjectsInCollider.Count; i++ )
		{
			var objectInCollider = ObjectsInCollider[i];
			var objectPos = objectInCollider.WorldPosition + WorldTransform.Forward * EachObjectTraceAddition;
			var myRight = WorldTransform.Right;
			var myLeft = WorldTransform.Left;

			var traceResult = Scene.Trace.Ray( EyeTransform.WorldPosition, objectPos).WithCollisionRules("visibility").IgnoreGameObjectHierarchy(this.GameObject.Parent.Parent).Run();
			var traceResultLeft = Scene.Trace.Ray( EyeTransform.WorldPosition, objectPos + myLeft * EachObjectHorizentalCastRange.x ).WithCollisionRules( "visibility" ).IgnoreGameObjectHierarchy( this.GameObject.Parent.Parent ).Run();
			var traceResultRight = Scene.Trace.Ray( EyeTransform.WorldPosition, objectPos + myRight * EachObjectHorizentalCastRange.y ).WithCollisionRules( "visibility" ).IgnoreGameObjectHierarchy( this.GameObject.Parent.Parent ).Run();
			var traceResultUp = Scene.Trace.Ray( EyeTransform.WorldPosition, objectPos + Vector3.Up * EachObjectVerticalCastRange.x ).WithCollisionRules( "visibility" ).IgnoreGameObjectHierarchy( this.GameObject.Parent.Parent ).Run();
			var traceResultDown = Scene.Trace.Ray( EyeTransform.WorldPosition, objectPos + Vector3.Down * EachObjectVerticalCastRange.y ).WithCollisionRules( "visibility" ).IgnoreGameObjectHierarchy( this.GameObject.Parent.Parent ).Run();

			if ( DebugSightSystem )
			{
				DebugOverlay.Line( new Line(EyeTransform.WorldPosition, objectPos), traceResult.Hit && traceResult.GameObject.Equals(objectInCollider) ? Color.Green : Color.Red, CheckInterval);
				DebugOverlay.Line( new Line( EyeTransform.WorldPosition, (objectPos + myLeft * EachObjectHorizentalCastRange.x)), traceResultLeft.Hit && traceResultLeft.GameObject.Equals(objectInCollider) ? Color.Green : Color.Red, CheckInterval );
				DebugOverlay.Line( new Line( EyeTransform.WorldPosition, (objectPos + myRight * EachObjectHorizentalCastRange.y)), traceResultRight.Hit && traceResultRight.GameObject.Equals( objectInCollider ) ? Color.Green : Color.Red, CheckInterval );
				DebugOverlay.Line( new Line( EyeTransform.WorldPosition, (objectPos + Vector3.Up * EachObjectVerticalCastRange.x) ), traceResultUp.Hit && traceResultUp.GameObject.Equals(objectInCollider) ? Color.Green : Color.Red, CheckInterval );
				DebugOverlay.Line( new Line( EyeTransform.WorldPosition, (objectPos + Vector3.Down * EachObjectVerticalCastRange.y) ), traceResultDown.Hit && traceResultDown.GameObject.Equals(objectInCollider) ? Color.Green : Color.Red, CheckInterval );
			}

			if ( await IsObjectInSightFromTraceResult(objectInCollider, i,  traceResult, traceResultUp, traceResultRight, traceResultLeft, traceResultDown) )
			{
				// In Sight.
			}
			else
			{
				// Not in sight
				if ( ObjectsInSight.Contains( objectInCollider ) )
					ForgotObject( objectInCollider, i );
			}

			
			if ( (i + 1) % AsyncNthObjectNextFrame == 0 )
			{
				await Task.Frame();
			}
			
			
		}

		IsAlreadyChecking = false;
	}

	private async Task<bool> IsObjectInSightFromTraceResult(GameObject objectInCollider, int IndexInList, params SceneTraceResult[] traceResults)
	{
		foreach ( var traceResult in traceResults ) {
			if ( traceResult.Hit && traceResult.GameObject.Equals( objectInCollider ) )
			{
				if ( !ObjectsInSight.Contains( objectInCollider ) )
				{
					// New detected.
					DetectedObject( objectInCollider );
				}
				return true;
			}
		}

		return false;
	}

	private void DetectedObject(GameObject detectedObject) {

		ObjectsInSight.Add( detectedObject );
		OnSensedObjectInSight?.Invoke( detectedObject );
	}

	private void ForgotObject( GameObject forgottenObject, int IndexInList = -1 )
	{
		ObjectsInSight.Remove( forgottenObject );
		/*if ( IndexInList < 0 )
			ObjectsInSight.Remove( forgottenObject );
		else
			ObjectsInSight.RemoveAt( IndexInList );*/

		OnSensedObjectOutOfSight?.Invoke( forgottenObject );
	}

	private void ForgetAllObjects()
	{
		foreach ( var item in ObjectsInSight )
			OnSensedObjectOutOfSight?.Invoke(item);
	}

	void ITriggerListener.OnTriggerEnter( GameObject other )
	{
		if ( ColliderOfThisNPC.IsValid() && other.Equals( ColliderOfThisNPC ) )
			return;

		ObjectsInCollider.Add( other );

		if ( DebugSightSystem )
		{
			Log.Info(other.Name + " entered " + GameObject.Name + "'s conesight");
		}
	}

	void ITriggerListener.OnTriggerExit( GameObject other )
	{
		if ( ColliderOfThisNPC.IsValid() && other.Equals( ColliderOfThisNPC ) )
			return;

		if ( ObjectsInSight.Contains( other ) )
			ForgotObject( other );

		// if ( ObjectsInCollider.Contains( other ) ) return;
		ObjectsInCollider.Remove( other );

		if ( DebugSightSystem )
		{
			Log.Info( other.Name + " exited " + GameObject.Name + "'s conesight" );
		}
	}

	protected override void OnDisabled()
	{
		this.GameObject.Enabled = false;
		collider.Enabled = false;
		ForgetAllObjects();
		ObjectsInCollider.Clear();
		ObjectsInSight.Clear();
	}

	protected override void OnEnabled()
	{
		this.GameObject.Enabled = true;
		collider.Enabled = true;
	}

	protected override void OnAwake()
	{
		TimeUntilAISenseCheck = CheckInterval;
	}
	
}
