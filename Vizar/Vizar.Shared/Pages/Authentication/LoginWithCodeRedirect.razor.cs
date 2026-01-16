using Microsoft.AspNetCore.Components;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Operations;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Operations;

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
                NavigationManager.NavigateTo(PageRouteNames.Login);
                return;
            }

            var user = await CommonData.LoadTableDataById<UserModel>(TableNames.User, int.Parse(Id));
            var isLoginWithCodeEnabled = bool.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.EnableLoginWithCode)).Value);

            if (!isLoginWithCodeEnabled)
            {
                NavigationManager.NavigateTo(PageRouteNames.Login);
                return;
            }

            var deviceId = await DataStorageService.SecureGetAsync(StorageFileNames.UserDeviceIdDataFileName);
            var codeExpiryMinutes = int.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.CodeExpiryMinutes)).Value);
            var currentDateTime = await CommonData.LoadCurrentDateTime();

            if (user is null ||
                user.LastCode != int.Parse(Code) ||
                user.LastCodeDateTime is null ||
                user.LastCodeDateTime.Value.AddMinutes(codeExpiryMinutes) < currentDateTime ||
                user.LastCodeDeviceId is null ||
                user.LastCodeDeviceId != deviceId)
            {
                NavigationManager.NavigateTo(PageRouteNames.Login);
                return;
            }

            await UserData.ResetInsertUser(user);
            await DataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(user));
            NavigationManager.NavigateTo(PageRouteNames.Dashboard, true);
        }
        catch
        {
            NavigationManager.NavigateTo(PageRouteNames.Login);
        }
    }
}