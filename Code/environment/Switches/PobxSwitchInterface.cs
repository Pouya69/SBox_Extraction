using Sandbox;

public interface IPobxSwitchInterface
{
	public void ToggleInteract( GameObject user );
	public void Released( GameObject user );
	public bool IsOn();
}
