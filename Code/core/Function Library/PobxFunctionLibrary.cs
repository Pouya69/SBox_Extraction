using Sandbox;

public static class PobxFunctionLibrary
{
	public static void ApplyDamage(DamageInfo damageInfo, GameObject Victim) {
		var actionSystemComp = Victim.GetComponent<ActionSystemComponent>();
		if (actionSystemComp.IsValid())
		{
			actionSystemComp.ApplyDamage( damageInfo.Attacker, damageInfo.Damage );
		}
	}

	public static void ApplyDirectionalDamage( DamageInfo damageInfo, GameObject Victim ) {
		var entity = Victim.GetComponentInChildren<Rigidbody>();
		Log.Info( "Victim: " + Victim.Name );
		if ( entity.IsValid())
		{
			// Log.Info( "Working" );
			entity.Sleeping = false;
			entity.ApplyImpulse( damageInfo.Damage * 5.0f * ((damageInfo.Position - damageInfo.Origin).Normal) );
			entity.Sleeping = false;
		}

		ApplyDamage( damageInfo, Victim );
	}
}
