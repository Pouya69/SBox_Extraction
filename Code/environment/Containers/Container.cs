using Sandbox;
using static Sandbox.Services.Inventory;

public class ContainerBase : Component
{
	[Property, Feature( "Properties" )] protected SoundEvent ContainerOpenSound { get; set; }
	[Property, Feature( "Properties" )] protected ActionSystemComponent ActionSystemComponent { get; set; }
	[Property, Feature( "Properties" )] protected GameObject ObjectSpawnPoint { get; set; }
	[Property, Feature( "Inventory" )] public List<FContainerItem> ContainerItems { get; set; }
	[Property, Feature( "Inventory" )] public float SpitOutDelay { get; protected set; } = 0.4f;

	public virtual bool IsContainerAlive => ActionSystemComponent.IsValid();

	protected override void OnAwake()
	{
		if ( !IsContainerAlive ) return;
		if ( ActionSystemComponent != null)
		{
			ActionSystemComponent.OnDeath += OnDeath;
			ActionSystemComponent.OnDamaged += OnDamaged;
		}
	}

	protected virtual void OnDamaged( GameObject attacker, GameObject gameObject, float health, float damage )
	{
		
	}

	protected override void OnDestroy()
	{
		if ( !IsContainerAlive ) return;
		if ( ActionSystemComponent != null)
		{
			ActionSystemComponent.OnDeath -= OnDeath;
			ActionSystemComponent.OnDamaged -= OnDamaged;
		}
	}

	protected virtual void OnDeath( GameObject obj )
	{
		if ( ContainerOpenSound.IsValid())
		{
			GameObject.PlaySound( ContainerOpenSound );
		}

		if ( ActionSystemComponent != null)
			ActionSystemComponent.Destroy();

		SpitOutObjects();
	}

	protected virtual async void SpitOutObjects() {
		for ( int i = ContainerItems.Count-1; i >= 0; i-- )
		{
			var item = ContainerItems[i];

			var objectPrefab = item.ObjectPrefab;
			var amountRange = item.Amount;
			var objectAmount = (item.AmountMultiplier > 0 ? item.AmountMultiplier : 1) * Random.Shared.Next( amountRange.x, amountRange.y);
			int cost = item.Cost;

			for ( int j = 0; j < objectAmount; j++ )
			{
				var gameObjectSpawned = objectPrefab.Clone( new CloneConfig() { StartEnabled = false, Transform = Transform.World.WithScale( 1 ) } );
				if ( cost > 0)
				{
					if ( gameObjectSpawned.GetComponent<InventoryGrabbableComponent>( true ) is var grabbableComp && grabbableComp != null )
					{
						grabbableComp.Cost = cost;
					}
					else if ( gameObjectSpawned.GetComponent<ConsumableBase>( true ) is var consumableComp && consumableComp != null )
					{
						consumableComp.Cost = cost;
					}
				}

				SpitOut( gameObjectSpawned );
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
	public FContainerItem()
	{
	}

	/// <summary>
	/// If true, for shops, it will not create them on 'refresh' if out of stock for them.
	/// </summary>
	public bool IsUnique { get; set; } = false;
	public int Cost { get; set; }

	/// <summary>
	/// For when we want items in stacks of 5 for example.
	/// </summary>
	public int AmountMultiplier { get; set; } = 1;

	public Vector2Int Amount { get; set; }
	public PrefabScene ObjectPrefab { get; set; }
}
