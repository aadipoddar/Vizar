using Vizar.Shared.Services;

using VizarLibrary.Models.Common;

namespace Vizar.Shared.Pages;

public partial class Dashboard
{
	private bool _isLoading = true;
	private UserModel _user;

	private string factor => FormFactor.GetFormFactor();
	private string platform => FormFactor.GetPlatform();

	protected override async Task OnInitializedAsync()
	{
		try
		{
			_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService);
			_isLoading = false;
		}
		catch (Exception ex)
		{

		}
	}
}