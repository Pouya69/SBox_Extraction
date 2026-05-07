using Sandbox;

public class GadgetBase : InventoryGrabbableComponent
{

	[Property, Feature( "Gadget" ), Group( "References" )] public SoundEvent UseSound { get; protected set; }
	public PobxPlayer Player { get; protected set; }

	public override void OnControl( PobxPlayer Player )
	{
		if ( Input.Pressed( "Attack1" ) )
		{
			UseGadget();
		}
	}

	protected virtual void UseGadget()
	{
		if ( UseSound.IsValid() )
		{
			var soundSpawned = GameObject.PlaySound( UseSound );
			if (soundSpawned.IsValid() && Player.IsLocalPlayer )
			{
				soundSpawned.SpacialBlend = 0.0f;
			}
		}
	}

	public virtual void InitializeGadget(PobxPlayer player) {
		this.GameObject.SetParent( player.Head );

		var playerEye = player.EyeTransform;

		DisableItem();
		this.WorldPosition = playerEye.Position + playerEye.Forward * 30.0f;
		this.Player = player;
	}

	public override void EnableItem()
	{
		this.WorldRotation = Rotation.Identity;
		this.GameObject.Enabled = true;
		// base.EnableItem();
	}

	public override void DisableItem()
	{
		this.EntityReference.GetRigidbody().Enabled = false;
		this.GameObject.Enabled = false;
		// base.DisableItem();
	}

	public virtual bool CanBeEquipped()
	{
		return pobxBaseInventoryItem.StackCount > 0;
	}

	public override bool WillBeDestroyedOnAddToInventory() => false;

}
