using System.Text.Json;
using AINovelWriter.Shared.Models;
using System.Threading;
using Radzen.Blazor;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using Microsoft.JSInterop;
using NovelWriterServiceStatic = AINovelWriter.Shared.Services.NovelWriterService;

namespace AINovelWriter.Web.Pages;
public partial class OutlinePage
{
    private bool _showOutline;
    private RadzenTextArea _textArea;
    private string _selectedText = "";

    private class ModifyOutlineSectionForm
    {
        public string? SelectedText { get; set; }
        public string? Instructions { get; set; }
        public AIModel AIModel { get; set; } = AIModel.Gpt41;
    }
    private ModifyOutlineSectionForm _modifyOutlineSectionForm = new();
    private async Task GenerateNovel(NovelOutline novelWriter)
    {
        IsBusy = true;

        StateHasChanged();
        AppState.NovelOutline = novelWriter;
        AppState.WriterModel = novelWriter.WriterAIModel;
        AppState.NovelInfo.Text = "";

        NavigationManager.NavigateTo("stream");
        await Task.Delay(1);

        //_isBusy = false;
        StateHasChanged();
    }
    private async Task OnSelect()
    {
        var selectedText = await JsRuntime.InvokeAsync<string>("getSelectedText", _textArea.Element);
        Console.WriteLine(selectedText);
        _selectedText = selectedText;
        _modifyOutlineSectionForm.SelectedText = selectedText;
        StateHasChanged();
    }
    private async Task ModifyOutlineSection(ModifyOutlineSectionForm form)
    {
        if (string.IsNullOrEmpty(form.SelectedText) || string.IsNullOrEmpty(form.Instructions)) return;

        var outline = AppState.NovelOutline.Outline;
        var selectedText = form.SelectedText;
        var instructions = form.Instructions;
        var modifiedOutlineSegment = await NovelWriterService.ModifyOutlineSection(outline, selectedText, instructions, form.AIModel);
        AppState.NovelOutline.Outline = outline?.Replace(selectedText, modifiedOutlineSegment);
        var chapterOutlineLines = NovelWriterServiceStatic.SplitMarkdownByHeaders(AppState.NovelOutline.Outline);
        HandleChapterOutline(JsonSerializer.Serialize(chapterOutlineLines));
        StateHasChanged();
    }
}
