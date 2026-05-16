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
		var entity = Victim.GetComponentInChildren<Rigidbody>(true);
		Log.Info( "Victim: " + Victim.Name );
		if ( entity.IsValid())
		{
			Log.Info( "Forcing..." );
			entity.Sleeping = false;
			entity.ApplyImpulse( damageInfo.Damage * 500.0f * ((damageInfo.Position - damageInfo.Origin).Normal) );
			entity.Sleeping = false;
		}
		else
		{
			
		}

		ApplyDamage( damageInfo, Victim );
	}
}
