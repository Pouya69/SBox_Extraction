using Sandbox;

public sealed class SceneManager: GameObjectSystem<SceneManager>, Component.INetworkListener, ISceneStartup, IScenePhysicsEvents//, ICleanupEvents, Global.ISaveEvents
{
	public SceneManager( Scene scene ) : base( scene ) { }
}
