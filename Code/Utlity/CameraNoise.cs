namespace Sandbox.CameraNoise;

public class CameraNoiseSystem : GameObjectSystem<CameraNoiseSystem>, ICameraSetup
{
	List<BaseCameraNoise> allNoises = new();

	public CameraNoiseSystem( Scene scene ) : base( scene )
	{
	}

	void ICameraSetup.PreSetup( CameraComponent cc )
	{
		foreach ( var noise in allNoises )
		{
			noise.Update();
		}

		allNoises.RemoveAll( x => x.IsDone );
	}

	void ICameraSetup.PostSetup( CameraComponent cc )
	{
		foreach ( var noise in allNoises )
		{
			noise.ModifyCamera( cc );
		}
	}

	public void Add( BaseCameraNoise noise ) {
		allNoises.Add( noise );
	}

}

public abstract class BaseCameraNoise
{
	public float LifeTime { get; protected set; }
	public float CurrentTime { get; protected set;  }
	public float Delta => CurrentTime.LerpInverse( 0, LifeTime, true );
	public float DeltaInverse => 1 - Delta;

	public virtual bool IsDone => CurrentTime >= LifeTime;

	public BaseCameraNoise()
	{
		CameraNoiseSystem.Current.Add( this );
	}

	public virtual void Update()
	{
		CurrentTime += Time.Delta;
	}

	public virtual void ModifyCamera( CameraComponent cc ) { }



}
