using NPBehave;
using Sandbox;
using Sandbox.Mapping;
using static Sandbox.Gizmo;

public class PobxButton : Component, IInteractable, IPobxSwitchInterface
{
	[Property, Group( "Button" )] private GameObject AnimatingObject { get; set; }

	[Property, Group( "Button" )] private Vector3 OnPosition { get; set; }
	private Vector3 _offPositon;
	private Vector3 _targetPosition;

	[Property, Group( "Button" )] public Button.ButtonMode ButtonMode { get; set; }
	[Property, Group( "Button" ), ShowIf( nameof( AutoReset ), true ), DefaultValue(1.0f)] public float ResetTime { get; set; } = 1.0f;
	[Property, Group( "Button" )] private float ButtonSpeed = 5.0f;

	[Property, Group( "Button" )]
	[ShowIf( nameof( ButtonMode ), Button.ButtonMode.Toggle )]
	[DefaultValue( true )]
	public bool AutoReset { get; set; } = true;
	[Property, Group( "Button" ), DefaultValue(true)] public bool IsButtonEnabled { get; private set; } = true;

	public TimeSince LastUse { get; private set; }

	private bool _shouldTurnOffNextFrame = false;
	private bool _isOn;
	private bool _isAnimating;
	private GameObject _lastUser;

	[Property, Group("Events")] public Doo OnPressed { get; set; }
	[Property, Group( "Events" )] public Doo OnReleased { get; set; }
	[Property, Group( "Events" )] public Doo OnTurnedOn { get; set; }
	[Property, Group( "Events" )] public Doo OnTurnedOff { get; set; }

	[Property, Group("Sounds")] public SoundEvent PressedSound { get; set; }
	[Property, Group( "Sounds" )] public SoundEvent ReleasedSound { get; set; }
	[Property, Group( "Sounds" )] public SoundEvent TurnOnSound { get; set; }
	[Property, Group( "Sounds" )] public SoundEvent TurnOffSound { get; set; }
	[Property, Group( "Sounds" )] public SoundEvent NotActiveSound { get; set; }


	public virtual void SetInitialValues()
	{
		_offPositon = this.AnimatingObject.LocalPosition;
	}

	protected override void OnAwake()
	{
		SetInitialValues();
		
	}

	protected override void OnFixedUpdate()
	{

		if ( IsOn() && AutoReset && ResetTime > 0.0f && LastUse >= ResetTime )
		{
			TurnOff();
			return;
		}
		if ( _shouldTurnOffNextFrame && IsOn() && !_isAnimating)
		{
			_shouldTurnOffNextFrame = false;
			TurnOff();
			return;
		}
		if ( _isAnimating )
			Animate();

	}

	public virtual void Animate()
	{
		this.AnimatingObject.LocalPosition = MathUtils.VInterpTo( this.AnimatingObject.LocalPosition, _targetPosition, Time.Delta, ButtonSpeed );
		if (this.AnimatingObject.LocalPosition.DistanceSquared(_targetPosition) <= 0.3f * 0.3f)
		{
			_isAnimating = false;
			Log.Info( "Animation finished/" );
		}
	}

	public void Interact( IInteractionComp interactionComponent )
	{
		ToggleInteract( interactionComponent.GetGameObject() );
	}

	public bool IsInteractable() => !_isAnimating;
	public bool IsPickUpTwoHanded() => false;
	public bool CanBePickedUp() => false;

	public void EnableButton()
	{
		IsButtonEnabled = true;
	}
	public void DisableButton()
	{
		IsButtonEnabled = false;
	}


	[Rpc.Host( NetFlags.Reliable )]
	public void ToggleInteract( GameObject user )
	{
		if ( PressedSound != null )
		{
			GameObject.StopAllSounds();
			GameObject.PlaySound( PressedSound );
		}

		// _isAnimating = true;

		if (!this.IsButtonEnabled)
		{
			return;
		}

		_lastUser = user;

		Run( OnPressed, delegate ( Doo.Configure c )
		{
			c.SetArgument( "user", user );
		} );
		
		switch ( ButtonMode )
		{
			case Button.ButtonMode.Toggle:
				if ( IsOn() && !AutoReset )
				{
					TurnOff();
				}
				else
				{
					TurnOn( user );
				}

				break;
			case Button.ButtonMode.Continuous:
				TurnOn( user );
				break;
			case Button.ButtonMode.Immediate:
				TurnOn( user );
				break;
		}
	}

	private void TurnOff()
	{
		
		if ( IsOn() )
		{
			_targetPosition = _offPositon;
			LastUse = 0f;
			_isAnimating = true;
			_isOn = false;
			if ( TurnOffSound != null )
			{
				GameObject.PlaySound( TurnOffSound );
			}

			Run( OnTurnedOff, delegate ( Doo.Configure c )
			{
				c.SetArgument( "user", _lastUser );
			} );
		}
	}

	private void TurnOn( GameObject user )
	{

		if ( !IsOn() && !_isAnimating )
		{
			_targetPosition = OnPosition;
			Log.Warning( "Turning on..." );
			_lastUser = user;
			LastUse = 0f;
			_isAnimating = true;
			_isOn = true;
			if ( TurnOnSound != null )
			{
				GameObject.PlaySound( TurnOnSound );
			}

			if ( IsButtonEnabled )
			{
				Run( OnTurnedOn, delegate ( Doo.Configure c )
				{
					c.SetArgument( "user", user );
				} );
			}

			

			if ( ButtonMode == Button.ButtonMode.Immediate )
			{
				_shouldTurnOffNextFrame = true;
			}
		}
	}

	public void Released() => Released( _lastUser );

	public void Released( GameObject user )
	{
		// _isAnimating = true;
		if ( !this.IsButtonEnabled )
		{
			if ( NotActiveSound != null)
			{
				GameObject.StopAllSounds();
				GameObject.PlaySound( NotActiveSound );
			}
			
			return;
		}

		if ( ButtonMode == Button.ButtonMode.Continuous && IsOn() )
		{
			TurnOff();
		}

		Run( OnReleased, delegate ( Doo.Configure c )
		{
			c.SetArgument( "user", user );
		} );
		GameObject.PlaySound( ReleasedSound );
		
	}

	public bool IsOn() => _isOn;
}
