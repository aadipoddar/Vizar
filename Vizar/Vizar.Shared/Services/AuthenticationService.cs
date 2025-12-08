using Microsoft.AspNetCore.Components;

using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Common;

namespace Vizar.Shared.Services;

public static class AuthenticationService
{
	public static async Task<UserModel> ValidateUser(
		IDataStorageService dataStorageService,
		NavigationManager navigationManager,
		IVibrationService vibrationService,
		Enum userRoles = null)
	{
		var userData = await dataStorageService.SecureGetAsync(StorageFileNames.UserDataFileName);
		if (string.IsNullOrEmpty(userData))
			await Logout(dataStorageService, navigationManager, vibrationService);

		var user = System.Text.Json.JsonSerializer.Deserialize<UserModel>(userData);
		if (user is null)
			await Logout(dataStorageService, navigationManager, vibrationService);

		var serverUser = await CommonData.LoadTableDataById<UserModel>(TableNames.User, user.Id);
		if (serverUser is null)
			await Logout(dataStorageService, navigationManager, vibrationService);

		if (!serverUser.Status)
			await Logout(dataStorageService, navigationManager, vibrationService);

		user = serverUser;
		await dataStorageService.SecureSaveAsync(StorageFileNames.UserDataFileName, System.Text.Json.JsonSerializer.Serialize(user));

		if (userRoles is null)
		{
			await dataStorageService.SecureRemove(StorageFileNames.UserDeviceIdDataFileName);
			return user;
		}

		var hasPermission = userRoles switch
		{
			UserRoles.Admin => user.Admin,
			UserRoles.Accounts => user.Accounts,
			UserRoles.Purchase => user.Purchase,
			_ => false
		};

		if (!hasPermission)
			await Logout(dataStorageService, navigationManager, vibrationService);

		await dataStorageService.SecureRemove(StorageFileNames.UserDeviceIdDataFileName);
		return user;
	}

	public static async Task Logout(IDataStorageService dataStorageService, NavigationManager navigationManager, IVibrationService vibrationService)
	{
		await dataStorageService.SecureRemoveAll();
		vibrationService.VibrateWithTime(500);
		navigationManager.NavigateTo(PageRouteNames.Login, true);
	}
}
