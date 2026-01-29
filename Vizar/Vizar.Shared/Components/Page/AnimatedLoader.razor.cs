using Microsoft.AspNetCore.Components;

namespace Vizar.Shared.Components.Page;

public partial class AnimatedLoader
{
    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public string? Label { get; set; }
}