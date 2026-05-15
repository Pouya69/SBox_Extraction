using Sandbox;


public static class Global
{
	public const int _playerRespawnAfterSeconds = 5;

	public static int PlayerRespawnAfterSeconds => _playerRespawnAfterSeconds;

	public interface IPlayerEvents : ISceneEvent<IPlayerEvents>
	{
		void OnPlayerSpawned() { }
		void OnPlayerDied() { }
	}
}
