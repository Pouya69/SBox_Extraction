using Sandbox;
using System.Numerics;
using static Global;

public class GameManager : GameObjectSystem<GameManager>, Component.INetworkListener, ISceneStartup, IScenePhysicsEvents
{
	public GameManager( Scene scene ) : base( scene ) { }

	private List<PobxPlayerState> Players = new();

	public bool IsResettingLevel { get; private set; }

	void ISceneStartup.OnHostInitialize()
	{
		if ( !Networking.IsActive )
		{
			Networking.CreateLobby( new Sandbox.Network.LobbyConfig() { Privacy = Sandbox.Network.LobbyPrivacy.Public, MaxPlayers = 32, Name = "Sandbox", DestroyWhenHostLeaves = true } );
		}
	}

	void Component.INetworkListener.OnActive( Connection channel )
	{
		channel.CanSpawnObjects = false;



		var playerData = CreatePlayerInfo( channel );
		SpawnPlayer( playerData );

		// Scene.Get<Chat>()?.AddSystemText( $"{channel.DisplayName} has joined the game", "👋" );
	}

	private PobxPlayerState CreatePlayerInfo(Connection channel )
	{
		var existingData = PobxPlayerState.For( channel );
		if ( existingData.IsValid() )
			return existingData;

		// New Player Info.
		var go = new GameObject(true, $"PlayerData - {channel.DisplayName}" );
		var data = go.AddComponent<PobxPlayerState>();
		data.SteamId = channel.SteamId;
		data.DisplayName = channel.DisplayName;
		data.PlayerId = channel.Id;

		go.NetworkSpawn( null );
		go.Network.SetOwnerTransfer( OwnerTransfer.Fixed );

		Players.Add(data);

		return data;
	}

	public void SpawnPlayer( Connection connection ) => SpawnPlayer( PobxPlayerState.For( connection ) );

	public void SpawnPlayer( PobxPlayerState playerState )
	{
		playerState.IsRespawning = false;
		var startLocation = FindSpawnLocation().WithScale(1.0f);

		var playerGo = GameObject.Clone( "/player/sbe_player.prefab", new CloneConfig { Name = playerState.DisplayName, StartEnabled = false, Transform = startLocation } );

		var player = playerGo.GetComponent<PobxPlayer>( true );
		player.Controller.WorldRotation = startLocation.Rotation;
		player.PlayerState = playerState;

		playerState.CurrentPlayerReference = player;

		var owner = Connection.Find( playerState.PlayerId );
		playerGo.NetworkSpawn( owner );

		//if ( player.IsLocalPlayer )
			//playerGo.Tags.Add( "local_player" );

		playerGo.Enabled = true;

		IPlayerEvents.Post(x => x.OnPlayerSpawned());

		// Log.Info( "WWWW" );
	}

	public void RespawnPlayer( Connection connection ) => RespawnPlayer( PobxPlayerState.For( connection ) );

	public bool AreAllPlayersDead()
	{
		foreach ( var playerState in Players )
		{
			if ( playerState.CurrentPlayerReference?.ActionSystemComp.IsAlive() ?? true )
				return false;
		}

		return true;
	}

	public async void RespawnPlayer( PobxPlayerState playerState )
	{
		if ( playerState.IsRespawning )
			return;

		playerState.IsRespawning = true;

		if ( !IsResettingLevel || AreAllPlayersDead() )
		{
			ResetLevel();
			return;
		}

		
		Log.Warning( "trying..." );

		await Task.Delay( Global.PlayerRespawnAfterSeconds * 1000 );

		if ( !playerState.IsRespawning )
		{
			// For when we reset the scene etc.
			return;
		}

		Log.Warning( "Working Respawn" );

		playerState.CurrentPlayerReference.DestroyGameObject();

		await Task.Delay( 20 );

		SpawnPlayer( playerState );
	}

	void ISceneStartup.OnClientInitialize()
	{
		// ResetLevel();
	}

	public async void ResetLevel()
	{
		IsResettingLevel = true;

		foreach ( var playerState in PobxPlayerState.All )
		{
			if ( !playerState.IsRespawning && !playerState.CurrentPlayerReference.ActionSystemComp.IsAlive() )
				playerState.CurrentPlayerReference.ActionSystemComp.Death();

			// RespawnPlayer( playerState );
		}

		await Task.Delay( SceneResetAfterSeconds * 1000 );

		LoadNewLevel( Scene.Source as SceneFile);
		IsResettingLevel = false;
		// Scene.Load( Scene.Source );
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();

		if ( spawnPoints.Length == 0 )
		{
			return Transform.Zero;
		}

		return Random.Shared.FromArray( spawnPoints ).Transform.World;
	}

	public void LoadNewLevel(SceneFile newScene) {
		var loadOptions = new SceneLoadOptions() { ShowLoadingScreen = true, IsAdditive = false };
		loadOptions.SetScene(newScene);
		Game.ChangeScene( loadOptions );
	}
}
