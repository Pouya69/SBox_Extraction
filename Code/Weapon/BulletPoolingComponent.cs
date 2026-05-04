using Sandbox;

public sealed class BulletPoolingComponent : Component
{
	[Property, RequireComponent] private Weapon Weapon { get; set; }
	[Property] private PrefabScene BulletPrefab;
	[Property] public int StartingBulletPoolCount { get; protected set; } = 30;

	private Queue<PobxBullet> AvailableBullets { get; set; } = new();

	protected override void OnStart()
	{
		SpawnBulletsAndAddToPool();
	}

	public void SpawnBulletsAndAddToPool()
	{
		if (IsAnyBulletAvailable())
		{
			// Clear the current pool.
			ClearPool();
		}

		for ( int i = 0; i < StartingBulletPoolCount; i++ )
		{

			var createdBullet = SpawnBullet(BulletPrefab, Weapon );

			if ( createdBullet != null )
				AddBulletToPool( createdBullet );
		}
	}

	public void AddBulletToPool(PobxBullet bullet)
	{
		AvailableBullets.Enqueue( bullet );
	}

	public static PobxBullet SpawnBullet(PrefabScene bulletPrefab, Weapon ownerWeapon)
	{
		var go = bulletPrefab.Clone( new CloneConfig { StartEnabled = false } );
		var bullet = go.GetComponent<PobxBullet>(true);
		bullet.InitializeBulletFirstTime( ownerWeapon );
		return bullet;
	}

	public void ClearPool()
	{
		foreach ( var bullet in AvailableBullets )
		{
			bullet.DestroyGameObject();
		}

		AvailableBullets.Clear();
	}

	public bool PopBulletFromPool(out PobxBullet bullet)
	{
		if ( !IsAnyBulletAvailable() ) {
			bullet = null;
			return false; 
		}

		bullet = AvailableBullets.Dequeue();

		return true;
	}

	public bool IsAnyBulletAvailable() => AvailableBullets.Count != 0;
}
