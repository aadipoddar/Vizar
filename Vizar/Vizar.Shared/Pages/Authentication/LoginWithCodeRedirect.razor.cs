using Microsoft.AspNetCore.Components;

using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Common;

namespace Vizar.Shared.Pages.Authentication;

public partial class LoginWithCodeRedirect
{
	[Parameter] public string Id { get; set; }
	[Parameter] public string Code { get; set; }

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		try
		{
			if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Code))
			{
				NavigationManager.NavigateTo("/login");
				return;
			}

			var user = await CommonData.LoadTableDataById<UserModel>(TableNames.User, int.Parse(Id));
			var isLoginWithCodeEnabled = bool.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.EnableLoginWithCode)).Value);

			if (!isLoginWithCodeEnabled)
			{
				NavigationManager.NavigateTo("/login");
				return;
			}

			var codeExpiryMinutes = int.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.CodeExpiryMinutes)).Value);
			var currentDateTime = await CommonData.LoadCurrentDateTime();

			if (user is null || user.LastCode != int.Parse(Code) || user.LastCodeDateTime is null || user.LastCodeDateTime.Value.AddMinutes(codeExpiryMinutes) < currentDateTime)
			{
				NavigationManager.NavigateTo("/login");
				return;
			}

			user.LastCode = null;
			user.LastCodeDateTime = null;
			user.FailedAttempts = 0;
			user.CodeResends = 0;
			await UserData.InsertUser(user);

			await DataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(user));
			NavigationManager.NavigateTo("/", true);
		}
		catch
		{
			NavigationManager.NavigateTo("/login");
		}
	}
}