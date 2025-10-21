using Syncfusion.Blazor.Inputs;

using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Common;

namespace Vizar.Shared.Pages.Authentication;

public partial class LoginPage
{
	private UserModel _user = new();

	private bool _isVerifying = false;

	private string _phoneEmail = string.Empty;
	private string _password = string.Empty;

	private string _passwordPlaceholder = "Enter password";

	private List<UserModel> _users = [];

	private SfTextBox _phoneEmailTextBox;
	private SfTextBox _passwordTextBox;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		try
		{
			await DataStorageService.SecureRemoveAll();
			await _phoneEmailTextBox.FocusAsync();
			_users = await CommonData.LoadTableData<UserModel>(TableNames.User);
		}
		catch (Exception ex)
		{
		}
	}

	private async Task OnPhoneEmailInput(InputEventArgs args)
	{
		_phoneEmail = args.Value;

		var user = _users.FirstOrDefault(u => u.Phone == _phoneEmail || u.Email == _phoneEmail);
		if (user is null)
		{
			_passwordPlaceholder = "Enter password";
			_user = new();
		}

		else
		{
			_user = user;
			_passwordPlaceholder = $"Enter password for {_user.Name}";
			await _passwordTextBox.FocusAsync();
		}

		StateHasChanged();
	}

	private async Task OnPasswordInput(InputEventArgs args)
	{
		_password = args.Value;

		if (_isVerifying)
			return;

		_isVerifying = true;

		_user = _users.FirstOrDefault(u => u.Phone == _phoneEmail || u.Email == _phoneEmail);
		if (_user is not null && _password == _user.Password && _user.Status)
		{
			await DataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(_user));
			VibrationService.VibrateWithTime(500);
			NavigationManager.NavigateTo("/");
		}

		_isVerifying = false;
	}

	private async Task OnLoginClick()
	{
		if (_isVerifying)
			return;

		_isVerifying = true;

		_user = _users.FirstOrDefault(u => u.Phone == _phoneEmail || u.Email == _phoneEmail);

		if (_user is null)
		{
			await _phoneEmailTextBox.FocusAsync();
			return;
		}

		if (_password != _user.Password)
		{
			await _passwordTextBox.FocusAsync();
			return;
		}

		if (!_user.Status)
		{
			await _phoneEmailTextBox.FocusAsync();
			return;
		}

		await DataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(_user));
		VibrationService.VibrateWithTime(500);
		NavigationManager.NavigateTo("/");

		_isVerifying = false;
	}
}