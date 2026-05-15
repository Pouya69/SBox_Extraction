using Sandbox;


public static class Global
{
	public const int _playerRespawnAfterSeconds = 5;
	public const int _sceneResetAfterSeconds = 5;

	public static int PlayerRespawnAfterSeconds => _playerRespawnAfterSeconds;
	public static int SceneResetAfterSeconds => _playerRespawnAfterSeconds;

	public interface IPlayerEvents : ISceneEvent<IPlayerEvents>
	{
		void OnPlayerSpawned() { }
		void OnPlayerDied() { }
	}
}
