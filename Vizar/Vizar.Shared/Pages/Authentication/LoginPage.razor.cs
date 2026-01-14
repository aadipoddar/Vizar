using Syncfusion.Blazor.Inputs;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Operations;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Common;

namespace Vizar.Shared.Pages.Authentication;

public partial class LoginPage : IAsyncDisposable
{
	private HotKeysContext _hotKeysContext;

	private UserModel _user = new();

	private bool _isVerifying = false;

	private string _phoneEmail = string.Empty;
	private string _password = string.Empty;

	private string _passwordPlaceholder = "Enter password";

	private bool _isLoginWithCodeEnabled = true;
	private int _maxLoginAttempts;

	private List<UserModel> _users = [];

	private SfTextBox _phoneEmailTextBox;
	private SfTextBox _passwordTextBox;

	private ToastNotification _toastNotification;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		try
		{
			_hotKeysContext = HotKeys.CreateContext()
				.Add(Code.Enter, OnLoginClick, "Login", Exclude.None);

			await DataStorageService.SecureRemoveAll();
			await _phoneEmailTextBox.FocusAsync();

			_maxLoginAttempts = int.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.MaxLoginAttempts)).Value);
			_isLoginWithCodeEnabled = bool.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.EnableLoginWithCode)).Value);

			_users = await CommonData.LoadTableData<UserModel>(TableNames.User);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Initializing Login Page", ex.Message, ToastType.Error);
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
			await UserData.ResetInsertUser(_user);
			await DataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(_user));
			NavigationManager.NavigateTo(PageRouteNames.Dashboard);
		}

		_isVerifying = false;
	}

	private async Task OnLoginClick()
	{
		if (_isVerifying)
			return;

		try
		{
			_isVerifying = true;

			_user = _users.FirstOrDefault(u => u.Phone == _phoneEmail || u.Email == _phoneEmail);

			if (_user is null)
			{
				await _phoneEmailTextBox.FocusAsync();
				await _toastNotification.ShowAsync("Login Failed", "No user found with the provided phone number or email.", ToastType.Error);
				return;
			}

			if (_password != _user.Password)
			{
				_user.FailedAttempts++;

				if (_user.FailedAttempts >= _maxLoginAttempts)
				{
					_user.Status = false;
					await UserData.InsertUser(_user);
					await _toastNotification.ShowAsync("Account Locked", "Your account has been locked due to multiple failed login attempts. Please contact support.", ToastType.Error);
					NavigationManager.NavigateTo(PageRouteNames.Login, true);
					return;
				}

				await UserData.InsertUser(_user);

				await _passwordTextBox.FocusAsync();
				await _toastNotification.ShowAsync("Login Failed", "Incorrect password. Please try again.", ToastType.Error);
				return;
			}

			if (!_user.Status)
			{
				await _phoneEmailTextBox.FocusAsync();
				await _toastNotification.ShowAsync("Login Failed", "This account is inactive. Please contact support.", ToastType.Error);
				return;
			}

			await UserData.ResetInsertUser(_user);
			await DataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(_user));
			VibrationService.VibrateWithTime(500);
			NavigationManager.NavigateTo(PageRouteNames.Dashboard);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Logging In", ex.Message, ToastType.Error);
		}
		finally
		{
			_isVerifying = false;
		}
	}

	private async Task OnForgotPasswordClick()
	{
		if (!_isLoginWithCodeEnabled)
			return;

		NavigationManager.NavigateTo(PageRouteNames.LoginWithCode);
	}

	public async ValueTask DisposeAsync()
	{
		if (_hotKeysContext is not null)
			await _hotKeysContext.DisposeAsync();

		GC.SuppressFinalize(this);
	}
}