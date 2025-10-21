using Vizar.Shared.Services;

namespace Vizar.Services;

public class UpdateService : IUpdateService
{
	public async Task<bool> CheckForUpdatesAsync(string githubRepoOwner, string githubRepoName, string currentVersion)
	{
#if ANDROID
		return await Vizar.Services.Android.AadiSoftUpdater.CheckForUpdates(githubRepoOwner, githubRepoName, currentVersion);
#else
		await Task.CompletedTask;
		// Feature will come soon for other platforms
		return false;
#endif
	}

	public async Task UpdateAppAsync(string githubRepoOwner, string githubRepoName, string setupAPKName, IProgress<int> progress = null)
	{
#if ANDROID
		await Vizar.Services.Android.AadiSoftUpdater.UpdateApp(githubRepoOwner, githubRepoName, setupAPKName, progress);
#else
		await Task.CompletedTask;
#endif
	}
}
