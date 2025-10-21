using Vizar.Shared.Services;

namespace Vizar.Web.Services;

public class FormFactor : IFormFactor
{
	public string GetFormFactor() => "Web";

	public string GetPlatform() => Environment.OSVersion.ToString();
}
