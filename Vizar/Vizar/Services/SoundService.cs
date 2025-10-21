using Plugin.Maui.Audio;

using Vizar.Shared.Services;

namespace Vizar.Services;

public class SoundService : ISoundService
{
	public async Task PlaySound(string soundFileName) =>
		AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(soundFileName)).Play();
}
