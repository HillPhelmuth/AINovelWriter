using Microsoft.AspNetCore.Components;
namespace AINovelWriter.Web.Components;
public partial class BusyOverlay : ComponentBase
{
    [Parameter]
    public string? Message { get; set; }
}
