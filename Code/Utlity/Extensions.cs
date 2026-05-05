public static class Extensions
{
	extension( Vector3 direction )
	{
		public Vector3 WithAimCone( float degrees )
		{
			var angle = Rotation.LookAt( direction );
			angle *= new Angles( Game.Random.Float( -degrees / 2.0f, degrees / 2.0f ), Game.Random.Float( -degrees / 2.0f, degrees / 2.0f ), 0 );
			return angle.Forward;
		}

		public Vector3 WithAimCone( float horizontalDegrees, float verticalDegrees )
		{
			var angle = Rotation.LookAt( direction );
			angle *= new Angles( Game.Random.Float( -verticalDegrees / 2.0f, verticalDegrees / 2.0f ), Game.Random.Float( -horizontalDegrees / 2.0f, horizontalDegrees / 2.0f ), 0 );
			return angle.Forward;
		}
	}
}
