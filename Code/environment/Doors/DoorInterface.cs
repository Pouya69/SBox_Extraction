using Sandbox;

public interface IDoor
{
	public bool IsOpen();

	/// <summary>
	/// If it is in the progress of opening and closing (animations etc.)
	/// </summary>
	/// <returns></returns>
	public bool IsInProgress();

	public void ToggleOpenDoor();
}
