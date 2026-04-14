using System;

public enum ECharacterEntityType
{
	[Icon("face")]
	PLAYER,
	[Icon("sentiment_extremely_dissatisfied")]
	[Description("For enemies without a specific purpose. Will just go hostile.")]
	ENEMY_BASIC,
}

public class ActionSystemComponent : Component
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
		if ( Health <= 0.0f )
			Death();
	}

	protected virtual void Death()
	{
		OnDeath?.Invoke( GameObject );
	}
	
	protected override void OnStart()
	{
		Health = MaxHealth;
		Damage = BaseDamage;
	}
}
