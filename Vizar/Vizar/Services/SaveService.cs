using Vizar.Shared.Services;

namespace Vizar.Services;

public partial class SaveService
{
	//Method to save document as a file and view the saved document.
	public partial string SaveAndView(string filename, string contentType, MemoryStream stream);
}

public class SaveAndViewService : ISaveAndViewService
{
	public async Task<string> SaveAndView(string filename, string contentType, MemoryStream stream)
	{
		SaveService saveService = new();
		await Task.CompletedTask;
		return saveService.SaveAndView(filename, contentType, stream);
	}
}
