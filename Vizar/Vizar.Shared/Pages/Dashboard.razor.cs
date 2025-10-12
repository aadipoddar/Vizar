namespace Vizar.Shared.Pages;

public partial class Dashboard
{
	private string factor => FormFactor.GetFormFactor();
	private string platform => FormFactor.GetPlatform();
}