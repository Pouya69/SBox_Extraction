using Sandbox;

public class ContainerBase : Component
{
	[Property, Feature( "Properties" )] protected SoundEvent ContainerOpenSound { get; set; }
	[Property, Feature( "Properties" )] protected ActionSystemComponent ActionSystemComponent { get; set; }
	[Property, Feature( "Properties" )] protected GameObject ObjectSpawnPoint { get; set; }
	[Property, Feature( "Inventory" )] public List<FContainerItem> ContainerItems { get; protected set; }
	[Property, Feature( "Inventory" )] public float SpitOutDelay { get; protected set; } = 0.4f;

	public virtual bool IsContainerAlive => ActionSystemComponent.IsValid();

	protected override void OnAwake()
	{
		if ( !IsContainerAlive ) return;
		ActionSystemComponent.OnDeath += OnDeath;
		ActionSystemComponent.OnDamaged += OnDamaged;
	}

	protected virtual void OnDamaged( GameObject attacker, GameObject gameObject, float health, float damage )
	{
		
	}

	protected override void OnDestroy()
	{
		if ( !IsContainerAlive ) return;
		ActionSystemComponent.OnDeath -= OnDeath;
		ActionSystemComponent.OnDamaged -= OnDamaged;
	}

	protected virtual void OnDeath( GameObject obj )
	{
		if ( ContainerOpenSound.IsValid())
		{
			GameObject.PlaySound( ContainerOpenSound );
		}

		ActionSystemComponent.Destroy();
		SpitOutObjects();
	}

	protected virtual async void SpitOutObjects() {
		for ( int i = ContainerItems.Count-1; i >= 0; i-- )
		{
			var objectPrefab = ContainerItems[i].ObjectPrefab;
			var objectAmount = ContainerItems[i].Amount;
			for ( int j = 0; j < objectAmount; j++ )
			{
				SpitOut( objectPrefab.Clone( new CloneConfig() { StartEnabled = false, Transform = Transform.World.WithScale(1) } ) );
				if ( SpitOutDelay > 0)
					await Task.DelaySeconds( SpitOutDelay );
			}
		}

	}

	/// <summary>
	/// By default it is disabled and has no transform.
	/// </summary>
	/// <param name="gameObject"></param>
	protected virtual void SpitOut(GameObject gameObject) {
		
	}
}

public struct FContainerItem
{
	public int Amount { get; set; }
	public PrefabScene ObjectPrefab { get; set; }
}
