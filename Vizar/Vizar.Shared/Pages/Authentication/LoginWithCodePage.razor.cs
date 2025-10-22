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

	private List<UserModel> _users = [];

	private SfTextBox _phoneEmailTextBox;
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
		if (user is null)
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
					await Mailing.SendMailCodeToUser(_user, _verificationCode.ToString());
					_codeSentTime = DateTime.Now;
					_codePlaceholder = $"Enter Code sent to {_user.Email} for {_user.Name}. The code is valid till {_codeSentTime.AddMinutes(10):hh:mm tt}";
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

	public async Task OnOtpInputChange(OtpInputEventArgs args)
	{
		_otpCode = args.Value;

		if (_isVerifying)
			return;
		try
		{
			_isVerifying = true;
			await ValidateCode();
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Verifying Code", ex.Message, "error");
		}
		finally
		{
			_isVerifying = false;
		}
	}

	private async Task OnLoginWithCodeClick()
	{
		if (_isVerifying)
			return;

		try
		{
			_isVerifying = true;
			await ValidateCode(true);
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

	private async Task ValidateCode(bool withToast = false)
	{
		if (!_isCodeSent)
		{
			if (withToast)
				await ShowToast("Code Not Sent", "Please send the code before attempting to log in.", "error");
			return;
		}

		if (string.IsNullOrEmpty(_otpCode) || _otpCode.Length != _otpInput.Length)
		{
			if (withToast)
			{
				_otpInput.FocusAsync();
				await ShowToast("Invalid Code", "Please enter the complete code sent to you.", "error");
			}
			return;
		}

		if (_user.Id == 0)
		{
			if (withToast)
			{
				await _phoneEmailTextBox.FocusAsync();
				await ShowToast("No User Selected", "Please enter a valid phone number or email address to send the code.", "error");
			}
			return;
		}

		if (_otpCode != _verificationCode.ToString())
		{
			if (withToast)
			{
				_otpInput.FocusAsync();
				await ShowToast("Login Failed", "Incorrect code. Please try again.", "error");
			}
			return;
		}

		if (!_user.Status)
		{
			if (withToast)
			{
				await _phoneEmailTextBox.FocusAsync();
				await ShowToast("Login Failed", "This account is inactive. Please contact support.", "error");
			}
			return;
		}

		if (_codeSentTime.AddMinutes(10) < DateTime.Now)
		{
			if (withToast)
			{
				_otpInput.FocusAsync();
				await ShowToast("Code Expired", "The code you entered has expired. Please request a new code.", "error");
			}
			return;
		}

		await DataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(_user));
		VibrationService.VibrateWithTime(500);
		NavigationManager.NavigateTo("/");
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

	private void OnBackToPasswordClick() =>
		NavigationManager.NavigateTo("/login");
}