using Microsoft.AspNetCore.Components;

using VizarLibrary.Models.Common;

namespace Vizar.Shared.Services;

public static class AuthenticationService
{
	public static async Task<UserModel?> ValidateUser(
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

		if (!user.Status)
			await Logout(dataStorageService, navigationManager, vibrationService);

		if (userRoles is null)
			return user;

		var hasPermission = userRoles switch
		{
			UserRoles.Admin => user.Admin,
			UserRoles.Inventory => user.Inventory,
			_ => false
		};

		if (!hasPermission)
			await Logout(dataStorageService, navigationManager, vibrationService);

		return user;
	}

	public static async Task Logout(IDataStorageService dataStorageService, NavigationManager navigationManager, IVibrationService vibrationService)
	{
		await dataStorageService.SecureRemoveAll();
		vibrationService.VibrateWithTime(500);
		navigationManager.NavigateTo("/Login", forceLoad: true);
	}
}
