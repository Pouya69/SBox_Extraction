using Sandbox;

public sealed class PobxPlayerState : Component
{
	/// <summary>
	/// The money player has.
	/// </summary>
	[Property] public int Credits { get; private set; } = 0;

	public event Action<int, int> OnCreditsChanged;

	[Property] public bool IsRespawning { get; set; } = false;
	[Property] public PobxPlayer CurrentPlayerReference { get; set; }

	[Property] public PobxPlayerInventory PlayerInventory { get; set; }

	[Property] public Guid PlayerId { get; set; }
	[Property] public long SteamId { get; set; } = -1L;
	[Property] public string DisplayName { get; set; }

	[Sync] public int Kills { get; set; }
	[Sync] public int Deaths { get; set; }

	[Sync] public bool IsGodMode { get; set; }

	public Connection Connection => Connection.Find( PlayerId );

	/// <summary>
	/// Is this player data me?
	/// </summary>
	public bool IsMe => PlayerId == Connection.Local.Id;

	/// <inheritdoc cref="Connection.Ping"/>
	public float Ping => Connection?.Ping ?? 0;

	/// <summary>
	/// Data for all players
	/// </summary>
	public static IEnumerable<PobxPlayerState> All => Game.ActiveScene.GetAll<PobxPlayerState>();

	/// <summary>
	/// Get player data for a player
	/// </summary>
	/// <param name="connection"></param>
	/// <returns></returns>
	public static PobxPlayerState For( Connection connection ) => connection == null ? default : For( connection.Id );

	/// <summary>
	/// Get player data for a player's id
	/// </summary>
	/// <param name="playerId"></param>
	/// <returns></returns>
	public static PobxPlayerState For( Guid playerId )
	{
		return All.FirstOrDefault( x => x.PlayerId == playerId );
	}

	public bool TakeCredits(int amountToTake)
	{
		int oldCredits = Credits;
		Credits = Math.Max( Credits - amountToTake, 0 );
		if ( Credits != oldCredits )
		{
			Log.Info( $"{DisplayName} credits changed. Old: {oldCredits}, New: {Credits}" );
			OnCreditsChanged?.Invoke( oldCredits, Credits );
			return true;
		}

		Log.Info( $"{DisplayName} did not have enough credits. Has: {Credits}, Needed: {amountToTake}" );

		return false;
	}

	public bool HasEnoughCredits( int amount ) => Credits >= amount;

	public void AddCredits( int amountToAdd)
	{
		int oldCredits = Credits;
		Credits += amountToAdd;

		Log.Info($"{DisplayName} credits changed. Old: {oldCredits}, New: {Credits}");

		OnCreditsChanged?.Invoke( oldCredits, Credits );
	}
}
