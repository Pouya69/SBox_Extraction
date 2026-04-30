using Sandbox;
using Sandbox.Citizen;
using Sandbox.Events;
using static Sandbox.Citizen.CitizenAnimationHelper;

public sealed class PlayerControllerExtension : Component
{
	// public record OnHealedEvent(GameObject HealedObject, float NewHealth, float HealedHealth) : IGameEvent;

	[Property, RequireComponent] public ActionSystemComponent ActionSystemComponent { get; private set; }
	[Property, RequireComponent] private PlayerInteractionComponent PlayerInteractionComponent { get; set; }
	public record OnPlayerDeathEvent(GameObject DiedObject) : IGameEvent;
	
	[Property] private PlayerController Controller;
	[Property] private SkinnedModelRenderer _modelRenderer;
	[Property] private GameObject WeaponAttachmentSocket;
	[Property] private float MeleeAttackCooldown { get; set; } = 1.0f;
	[Property] private float ResetPoseTime { get; set; } = 4.0f;
	public TimeUntil NextAttack;
	private TimeUntil _resetPose;
	
	private Weapon CurrentWeaponEquipped { get; set; }
	public bool HasWeaponEquipped => CurrentWeaponEquipped != null;

	protected override void OnStart()
	{
		ActionSystemComponent.OnDamaged += this.OnDamaged;
		ActionSystemComponent.OnDeath += this.OnDeath;
	}

	public void GiveWeapon( Weapon weapon )
	{
		// if ( CurrentWeaponEquipped is not null )
			// PlayUnequipWeaponAnimation();
		// _anim.wea
		CurrentWeaponEquipped = weapon;
		CurrentWeaponEquipped.ToggleWeaponPhysics( false );
		CurrentWeaponEquipped.GameObject.Parent = WeaponAttachmentSocket;
		CurrentWeaponEquipped.WorldPosition = WeaponAttachmentSocket.WorldPosition;
		CurrentWeaponEquipped.WorldRotation = WeaponAttachmentSocket.WorldRotation;
		CurrentWeaponEquipped.WorldScale = WeaponAttachmentSocket.WorldScale;
		
		SwitchToWeaponAnimation( weapon );
	}

	public void Attack()
	{
		if ( !HasWeaponEquipped )
		{
			// Setting hold mode to melee attack
			_modelRenderer.Set( "holdtype", 5 );
			_resetPose = ResetPoseTime;
		}
		CurrentWeaponEquipped.Shoot();
		PlayAttackAnimation();
	}

	public void Reload()
	{
		CurrentWeaponEquipped.Reload();
	}

	protected override void OnFixedUpdate()
	{
		if ( Input.Pressed( "Reload" ) )
		{
			if ( HasWeaponEquipped )
				Reload();
		}
		if (Input.Pressed( "Attack1" ) && NextAttack)
		{
			NextAttack = HasWeaponEquipped ? CurrentWeaponEquipped.AttackCooldown : MeleeAttackCooldown;
			Attack();
		}
		else if (Input.Released( "Attack1" ) )
		{
			if ( HasWeaponEquipped )
				CurrentWeaponEquipped.StopShoot();
		}

		if (Input.Pressed("Use"))
		{
			PlayerInteractionComponent.AttemptInteract();
		}

		if ( !HasWeaponEquipped && _resetPose)
		{
			ResetAnimationHoldType();
		}
	}

	public void PlayDeathAnimation() => _modelRenderer.Set("b_died", true);
	
	public void PlayDamagedAnimation() =>  _modelRenderer.Set("b_hit", true);

	public void PlayAttackAnimation() => _modelRenderer.Set("b_attack", true);

	public void PlayReloadAnimation() => _modelRenderer.Set("b_reload", true);

	public void PlayHealAnimation() => _modelRenderer.Set("b_reload", true);
	public void ResetAnimationHoldType() => SetAnimationHoldType( CitizenAnimationHelper.HoldTypes.None );
	public void SetAnimationHoldType(CitizenAnimationHelper.HoldTypes holdType) => _modelRenderer.Set( "holdtype", holdType.AsInt() );
	public void SetHandedHoldType( Hand hand ) => _modelRenderer.Set( "holdtype_handedness", hand.AsInt() );

	private void SwitchToWeaponAnimation( Weapon weapon )
	{
		SetAnimationHoldType( weapon.GetWeaponType());
		SetHandedHoldType(weapon.GetWeaponHoldType());
	}

	public void OnDamaged( GameObject Attacker, GameObject Victim, float NewHealth, float DamageApplied )
	{
		PlayDamagedAnimation();
	}

	public void OnHealed( GameObject HealedObject, float NewHealth, float HealedHealth )
	{
		throw new System.NotImplementedException();
	}

	public void OnDeath( GameObject DiedObject )
	{
		PlayDeathAnimation();
		GameObject.Dispatch( new OnPlayerDeathEvent(GameObject) );  // For the whole scene
	}

	public void OnAddedDamage( float NewDamage, float AdditionPercentage )
	{
		throw new System.NotImplementedException();
	}
}
