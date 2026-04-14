using System;
using NPBehave;
using Exception = System.Exception;

namespace Sandbox.BehaviorTreeVisualizer;

public class BlackboardProperty : SerializedProperty
{
	private string Key { get; set; }
	private Blackboard Data { get; set; }

	public override string Name => Key;
	public override string DisplayName => Key;

	public bool HasValue => Data.IsSet( Key );
	public object Value => Data.Get( Key );
	
	public override Type PropertyType => Data.Get( Key ).GetType();

	public override SerializedObject Parent => Data.GetSerialized();

	public BlackboardProperty( string key, Blackboard data )
	{
		Key = key;
		Data = data;
	}
	
	public override void SetValue<T>( T value )
	{
		Data[Key] = value;
	}

	public override T GetValue<T>( T defaultValue = default(T) )
	{
		return HasValue ? Data.Get<T>( Key ) : defaultValue;
	}

	// Not sure if this is correct, just stole it from action graph code
	public override bool TryGetAsObject( out SerializedObject obj )
	{
		obj = null;
		var description = EditorTypeLibrary.GetType( PropertyType );

		if ( description == null )
		{
			return false;
		}

		try
		{
			if ( !PropertyType.IsValueType )
			{
				if ( !HasValue )
					return false;
				obj = EditorTypeLibrary.GetSerializedObject( Value );
				return true;
			}
			
			obj = EditorTypeLibrary.GetSerializedObject( () => HasValue && Value is not null ? Value :  Activator.CreateInstance( PropertyType ),
				description, this );
			return true;
		}
		catch ( Exception e )
		{
			Log.Warning( e );
			obj = null;
			return false;
		}
	}
}
