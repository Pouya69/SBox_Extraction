public static class GamePreferences
{
	[ConVar( "pobx.screenshake", ConVarFlags.Saved )]
	[Range( 0.1f, 2f ), Step( 0.1f ), Group( "Camera" )]
	public static float ScreenShakeMultiplier { get; set; } = 0.3f;
}
