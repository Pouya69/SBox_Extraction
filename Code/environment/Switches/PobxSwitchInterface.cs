using Sandbox;

public interface IPobxSwitchInterface
{
	public void SetInitialValues();
	public void ToggleInteract( GameObject user );
	public void Released( GameObject user );
	public bool IsOn();
	public void Animate();
}
