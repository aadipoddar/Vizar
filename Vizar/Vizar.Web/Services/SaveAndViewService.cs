using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Vizar.Shared.Services;

namespace Vizar.Web.Services;

public class SaveAndViewService(IJSRuntime jsRuntime) : ISaveAndViewService
{
	[Inject] private IJSRuntime JSRuntime { get; set; } = jsRuntime;

	public async Task<string> SaveAndView(string fileName, string contentType, MemoryStream stream)
	{
		if (contentType == "application/pdf")
			await JSRuntime.InvokeVoidAsync("savePDF", Convert.ToBase64String(stream.ToArray()), fileName);
		else
			await JSRuntime.InvokeVoidAsync("saveExcel", Convert.ToBase64String(stream.ToArray()), fileName);

		return fileName;
	}
}
