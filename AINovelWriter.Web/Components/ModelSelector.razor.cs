using AINovelWriter.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AINovelWriter.Web.Components;
public partial class ModelSelector
{
    [Parameter]
    public AIModel SelectedModel { get; set; }

    private AIModel _model;
    [Parameter]
    public EventCallback<AIModel> SelectedModelChanged { get; set; }

    protected override void OnParametersSet()
    {
        _model = SelectedModel;
        //Console.WriteLine($"Selected Model: {SelectedModel}");
        StateHasChanged();
        base.OnParametersSet();
    }

    private void HandleChange(AIModel model)
    {
        SelectedModel = model;
        SelectedModelChanged.InvokeAsync(SelectedModel);
    }
}
