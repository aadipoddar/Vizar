using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Notifications;

using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting;
using VizarLibrary.Models.Common;

namespace Vizar.Shared.Pages.Authentication;

public partial class LoginWithCodePage
{
	private UserModel _user = new();

	private bool _isVerifying = false;

	private bool _isCodeSent = false;
	private bool _isEmail = false;

	private string _phoneEmail = string.Empty;
	private string _otpCode = string.Empty;
	private DateTime _codeSentTime;

	private string _codePlaceholder = "Enter Code";

	private int _verificationCode;

	private string _newPassword = string.Empty;

	private bool _isLoginWithCodeEnabled = false;
	private bool _isEnabledUsersResetPassword = false;
	private int _maxLoginAttempts;
	private int _codeResendLimit;
	private int _codeExpiryMinutes;

	private List<UserModel> _users = [];

	private SfTextBox _phoneEmailTextBox;
	private SfTextBox _newPasswordTextBox;
	private SfOtpInput _otpInput;

	private string _errorTitle = string.Empty;
	private string _errorMessage = string.Empty;

	private string _successTitle = string.Empty;
	private string _successMessage = string.Empty;

	private SfToast _sfSuccessToast;
	private SfToast _sfErrorToast;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		try
		{
			await DataStorageService.SecureRemoveAll();
			await _phoneEmailTextBox.FocusAsync();
			_users = await CommonData.LoadTableData<UserModel>(TableNames.User);

			_isLoginWithCodeEnabled = bool.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.EnableLoginWithCode)).Value);

			if (!_isLoginWithCodeEnabled)
				NavigationManager.NavigateTo("/login", true);

			_isEnabledUsersResetPassword = bool.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.EnableUsersToResetPassword)).Value);
			_maxLoginAttempts = int.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.MaxLoginAttempts)).Value);
			_codeResendLimit = int.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.CodeResendLimit)).Value);
			_codeExpiryMinutes = int.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.CodeExpiryMinutes)).Value);
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Initializing Login Page", ex.Message, "error");
		}
	}

	private async Task OnSendCodeClick()
	{
		if (_isVerifying)
			return;

		if (string.IsNullOrEmpty(_phoneEmail))
		{
			await ShowToast("Invalid Input", "Please enter a valid phone number or email address.", "error");
			return;
		}

		_isEmail = _phoneEmail.Contains('@');
		VibrationService.VibrateHapticLongPress();

		var user = _users.FirstOrDefault(u => u.Phone == _phoneEmail || u.Email == _phoneEmail);
		if (user is null || user.Status == false)
		{
			await ShowToast("No User Found", "No user found with the provided phone number or email.", "error");
			_codePlaceholder = "Enter Code";
			_user = new();
		}

		else
		{
			try
			{
				_isVerifying = true;
				_user = user;

				if (_isEmail)
				{
					_verificationCode = new Random().Next(100000, 999999);

					_user.CodeResends++;

					if (_user.CodeResends >= _codeResendLimit)
					{
						_user.Status = false;
						await UserData.InsertUser(_user);
						await ShowToast("Resend Limit Exceeded", "You have exceeded the maximum number of code resends. Your account has been locked. Please contact support.", "error");
						NavigationManager.NavigateTo("/login", true);
						return;
					}

					_user.LastCode = _verificationCode;
					_user.LastCodeDateTime = await CommonData.LoadCurrentDateTime();
					await UserData.InsertUser(_user);

					await Mailing.SendMailCodeToUser(_user, _verificationCode.ToString(), _codeExpiryMinutes);
					_codeSentTime = await CommonData.LoadCurrentDateTime();
					_codePlaceholder = $"Enter Code sent to {_user.Email} for {_user.Name}. The code is valid till {_codeSentTime.AddMinutes(_codeExpiryMinutes):hh:mm tt}";
				}

				else
				{
					throw new NotImplementedException("Phone code sending not implemented.");
					// _codePlaceholder = $"Enter Code sent to {_user.Phone} for {_user.Name}";
				}

				await ShowToast("Code Sent Successfully", _isEmail ? $"A code has been sent to {_user.Email}" : $"A code has been sent to {_user.Phone}", "success");

				_otpInput.FocusAsync();
				_isCodeSent = true;
			}
			catch (Exception ex)
			{
				await ShowToast("An Error Occurred While Sending Code", ex.Message, "error");
				_codePlaceholder = "Enter Code";
			}
			finally
			{
				_isVerifying = false;
			}
		}
	}

	private async Task OnLoginWithCodeClick()
	{
		if (_isVerifying)
			return;

		try
		{
			_isVerifying = true;

			if (!_isCodeSent)
			{
				await ShowToast("Code Not Sent", "Please send the code before attempting to log in.", "error");
				return;
			}

			if (string.IsNullOrEmpty(_otpCode) || _otpCode.Length != _otpInput.Length)
			{
				_otpInput.FocusAsync();
				await ShowToast("Invalid Code", "Please enter the complete code sent to you.", "error");
				return;
			}

			if (_user.Id == 0)
			{
				await _phoneEmailTextBox.FocusAsync();
				await ShowToast("No User Selected", "Please enter a valid phone number or email address to send the code.", "error");
				return;
			}

			if (!_user.Status)
			{
				await _phoneEmailTextBox.FocusAsync();
				await ShowToast("Login Failed", "This account is inactive. Please contact support.", "error");
				return;
			}

			if (_otpCode != _verificationCode.ToString())
			{
				_user.FailedAttempts++;
				_user.LastCodeDateTime = null;
				_user.LastCode = null;

				if (_user.FailedAttempts >= _maxLoginAttempts)
				{
					_user.Status = false;
					await UserData.InsertUser(_user);
					await ShowToast("Account Locked", "Your account has been locked due to multiple failed login attempts. Please contact support.", "error");
					NavigationManager.NavigateTo("/login", true);
					return;
				}

				await UserData.InsertUser(_user);

				_otpInput.FocusAsync();
				await ShowToast("Login Failed", "Incorrect code. Please try again.", "error");
				return;
			}

			if (_codeSentTime.AddMinutes(_codeExpiryMinutes) < await CommonData.LoadCurrentDateTime())
			{
				_otpInput.FocusAsync();
				await ShowToast("Code Expired", "The code you entered has expired. Please request a new code.", "error");
				return;
			}

			_user.FailedAttempts = 0;
			_user.CodeResends = 0;
			_user.LastCodeDateTime = null;
			_user.LastCode = null;

			if (!string.IsNullOrEmpty(_newPassword))
			{
				if (!_isEnabledUsersResetPassword)
				{
					await _newPasswordTextBox.FocusAsync();
					await ShowToast("Password Reset Disabled", "Password reset functionality is disabled. Please contact support.", "error");
				}
				else
				{
					if (_newPassword.Length < 6)
					{
						await _newPasswordTextBox.FocusAsync();
						await ShowToast("Weak Password", "The new password must be at least 6 characters long.", "error");
						return;
					}
				}

				_user.Password = _newPassword;
			}

			await UserData.InsertUser(_user);

			await DataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(_user));
			VibrationService.VibrateWithTime(500);
			NavigationManager.NavigateTo("/");
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Logging In", ex.Message, "error");
		}
		finally
		{
			_isVerifying = false;
		}
	}

	private async Task ShowToast(string title, string message, string type)
	{
		VibrationService.VibrateWithTime(200);

		if (type == "error")
		{
			_errorTitle = title;
			_errorMessage = message;
			await _sfErrorToast.ShowAsync(new()
			{
				Title = _errorTitle,
				Content = _errorMessage
			});
		}

		else if (type == "success")
		{
			_successTitle = title;
			_successMessage = message;
			await _sfSuccessToast.ShowAsync(new()
			{
				Title = _successTitle,
				Content = _successMessage
			});
		}
	}
}