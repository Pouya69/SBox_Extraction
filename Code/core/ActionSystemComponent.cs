using System;

public class ActionSystemComponent : Component, Component.IDamageable
{
	public event Action<GameObject, GameObject, float, float> OnDamaged;
	public event Action<GameObject, float, float> OnHealed;
	public event Action<GameObject> OnDeath;
	public event Action<float, float> OnAddedDamage;
	
	[Range(10.0f, 800.0f), Step(10.0f)]
	[Property] public float MaxHealth {get; set;}
	private float _health;
	
	[Property] public float BaseDamage {get; set;}
	private float _damage;

	public bool IsAlive() => Health > 0;

	public float Health
	{
		get => _health;
		set => UpdateHealth(value);
	}

	public float Damage
	{
		get => _damage;
		set => AddDamage( value - _damage );
	}

	public void AddDamage( float additionPercentage01 )
	{
		_damage += additionPercentage01 * _damage;
		OnAddedDamage?.Invoke( Damage, additionPercentage01 );
	}

	[Group("Debug")] [Button("Hurt 10", "😭")]
	private void DebugDamage()
	{
		ApplyDamage( null, 10.0f );
	}
	
	[Group("Debug")] [Button("Heal 10", "❤️")]
	private void DebugHeal()
	{
		ApplyDamage( null,-10.0f );
	}

	public virtual void ApplyDamage(GameObject Attacker, float Damage)
	{
		Log.Info( "Applying damage..." );
		if ( Damage > 0.0f )
		{
			// Damaged.
			UpdateHealth(Health - Damage);
			OnDamaged?.Invoke( Attacker, GameObject, Health, Damage );
		}
		else if ( Damage < 0.0f )
		{
			// Healed.
			UpdateHealth(Health - Damage);
			OnHealed?.Invoke( GameObject, Health, Damage );
			
		}
	}
	
	private void UpdateHealth(float NewHealth)
	{
		_health = MathX.Clamp( NewHealth, 0.0f, MaxHealth );
		Log.Info( _health );
		if ( Health <= 0.0f )
			Death();
	}

	public void Heal( GameObject instigator, float healAmount ) => ApplyDamage( instigator, -healAmount );
	public bool CanBeHealed(bool isExtra = false) => isExtra ? Health < 2 * MaxHealth : Health < MaxHealth;


	protected virtual void Death()
	{
		OnDeath?.Invoke( GameObject );
	}
	
	protected override void OnStart()
	{
		Health = MaxHealth;
		Damage = BaseDamage;
	}

	public void OnDamage( in DamageInfo damage )
	{
		Health -= damage.Damage;
	}
}
