using Sandbox;
using System.Numerics;

public class DeployableGadget : GadgetBase, Component.ICollisionListener
{
	/// <summary>
	/// The actual prefab to spawn when the gadget is used.
	/// </summary>
	[Property, Feature( "Gadget" ), Group( "References" )] public PrefabScene GadgetWorldPrefab { get; protected set; }

	public bool IsDeploying { get; protected set; }
	public bool IsDropFromUsing = false;

	// [Property, Feature( "Gadget" )] public float DespawnAfterSeconds { get; set; } = 0.0f;

	protected override void UseGadget()
	{
		base.UseGadget();

		IsDropFromUsing = true;
		var playerInventory = pobxBaseInventoryItem.Inventory as PobxPlayerInventory;

		if ( this.pobxBaseInventoryItem.StackCount <= 1 )
		{
			// IsDropFromUsing = false;
			// IsDeploying = false;
			Player.InventoryComponent.DisableGadget();
			// this.GameObject.Enabled = false;
			// DestroyGameObject();
			Log.Info( "Disabled com" );
			playerInventory.RemoveItemAndDropFromInventory( this.pobxBaseInventoryItem );
		}
		else
		{
			this.pobxBaseInventoryItem.StackCount--;
			InventoryWasModified();
			if (!CanBeEquipped())
			{
				DisableItem();
			}
		}

		
		

		// if ( DespawnAfterSeconds > 0.0f )
		// Invoke( DespawnAfterSeconds, Despawn );


	}

	public override void ItemRemovedFromInventory( PobxPlayer player )
	{
		InventoryWasModified();
	}

	private void InventoryWasModified()
	{
		EnableItem();
		var rb = EntityReference.GetRigidbody();
		if ( !rb.IsValid() )
		{
			return;
		}

		this.GameObject.SetParent( null );

		rb.Enabled = true;
		rb.MotionEnabled = true;

		var forward = Player.EyeTransform.Forward;
		EntityReference.LaunchEntity( DeployThrowStrength * forward );

		if ( !IsDropFromUsing )
		{
			return;
		}

		IsDeploying = true;
	}

	protected virtual void DeployGadget(Vector3 pos, Rotation rot)
	{
		var gadgetSpawned = GadgetWorldPrefab.Clone( new CloneConfig() { StartEnabled = false } );
		gadgetSpawned.WorldPosition = pos;
		gadgetSpawned.WorldRotation = rot;

		gadgetSpawned.Enabled = true;

		if ( this.pobxBaseInventoryItem.StackCount <= 0 )
		{
			IsDropFromUsing = false;
			IsDeploying = false;
			Player.InventoryComponent.DisableGadget();
			this.GameObject.Enabled = false;
			DestroyGameObject();
			Log.Info( "Disabled com" );
		}
		else
		{
			IsDropFromUsing = false;
			IsDeploying = false;
			this.GameObject.Enabled = false;
			InitializeGadget( Player );
		}
	}

	protected void DeployGadget( Vector3 pos, Angles rot ) => DeployGadget( pos, Rotation.From( rot ) );

	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( !IsDeploying || !IsDropFromUsing || collision.Other.GameObject.Equals(Player) ) return;

		var pos = collision.Contact.Point;
		var endPos = pos + Vector3.Down * 20.0f;
		// var rot = .EulerAngles;

		var rot =	Rotation.LookAt( Vector3.Forward, collision.Contact.Normal );

		/*
		var traceResult = Scene.Trace.Ray( pos, endPos ).WithTag( "solid" ).UseHitPosition().Run();
		if ( traceResult.Hit )
		{
			pos = traceResult.HitPosition;
			rot = traceResult.Normal.EulerAngles;
			DebugOverlay.Line( new Line( pos, traceResult.Normal, 50.0f ), Color.Blue, 10 );
		}
		*/
		DeployGadget(pos, rot);
	}

	public override bool IsInteractable() => !IsDropFromUsing && !IsDeploying;

}
