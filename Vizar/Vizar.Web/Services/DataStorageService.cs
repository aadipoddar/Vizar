using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

using Vizar.Shared.Services;

using VizarLibrary.DataAccess;
using VizarLibrary.Models.Common;

namespace Vizar.Web.Services;

public class DataStorageService(ProtectedLocalStorage protectedLocalStorage) : IDataStorageService
{
	private readonly ProtectedLocalStorage _protectedLocalStorage = protectedLocalStorage;

	public async Task SecureSaveAsync(string key, string value) =>
		await _protectedLocalStorage.SetAsync(key, value);

	public async Task<string?> SecureGetAsync(string key) =>
		(await _protectedLocalStorage.GetAsync<string>(key)).Value;

	public async Task SecureRemove(string key) =>
		await _protectedLocalStorage.DeleteAsync(key);

	public async Task SecureRemoveAll()
	{
		await _protectedLocalStorage.DeleteAsync(StorageFileNames.UserDataFileName);

		//await LocalRemove(StorageFileNames.OrderDataFileName);
	}


	public async Task<bool> LocalExists(string key)
	{
		await Task.CompletedTask;
		return File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Secrets.DatabaseName, key));
	}

	public async Task LocalSaveAsync(string key, string value)
	{
		var directoryPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			Secrets.DatabaseName);

		Directory.CreateDirectory(directoryPath);

		var filePath = Path.Combine(directoryPath, key);
		await File.WriteAllTextAsync(filePath, value);
	}

	public async Task<string?> LocalGetAsync(string key)
	{
		if (await LocalExists(key))
			return await File.ReadAllTextAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Secrets.DatabaseName, key));

		return null;
	}

	public async Task LocalRemove(string key)
	{
		await Task.CompletedTask;
		var filePath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			Secrets.DatabaseName,
			key);

		if (File.Exists(filePath))
			File.Delete(filePath);
	}
}
