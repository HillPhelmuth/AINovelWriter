using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;
using static AINovelWriter.Shared.Models.EnumHelpers;

namespace AINovelWriter.Web.Pages;
public partial class EditNovel
{
    private List<ChapterOutline> ChapterOutlines => AppState.NovelInfo.ChapterOutlines;
    private static Dictionary<AIModel, string> AIModelDescriptions => GetEnumsWithDescriptions<AIModel>().ToDictionary(x => x.Key, y => y.Value);
    private class EditNovelForm
    {
        public ChapterOutline? ChapterOutline { get; set; }
        public string? Notes { get; set; }
        public AIModel AIModel { get; set; }
    }
    private EditNovelForm _editNovelForm = new();
    private Feedback? _feedback;
    private class ApplySuggestionForm
    {
        public Feedback? Feedback { get; set; }
        public ChapterOutline? ChapterOutline { get; set; }
        public AIModel AIModel { get; set; }
    }
    private ApplySuggestionForm _applySuggestionForm = new();
    private bool _isBusy;
    private string _originalText = "";
    private async void GetFeedback(EditNovelForm form)
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var text = form.ChapterOutline?.FullText;
        _originalText = text;
        var feedback = await NovelWriterService.ProvideRewriteFeedback(text, form.AIModel, form.Notes);
        _applySuggestionForm.Feedback = feedback;
        _applySuggestionForm.ChapterOutline = form.ChapterOutline;
        _isBusy = false;
        StateHasChanged();
    }
    private string _rewrite = "";
    private bool _isRewriting;
    private async Task ApplySuggestions(ApplySuggestionForm form)
    {
        _isRewriting = true;
        StateHasChanged();
        await Task.Delay(1);
        var chapterRewrite = await NovelWriterService.RewriteChapter(form.ChapterOutline!, form.Feedback!, form.AIModel);
        _rewrite = chapterRewrite;
        //AppState.NovelInfo.ChapterOutlines.First(x => x.Title == form.ChapterOutline.Title).FullText = chapterRewrite;

        //AppState.NovelInfo.Text = string.Join("\n\n", ChapterOutlines.Select(x => x.FullText));
        _isRewriting = false;
        StateHasChanged();
    }

    private void ApplyRewrite()
    {
        AppState.NovelInfo.ChapterOutlines.First(x => x.Title == _applySuggestionForm.ChapterOutline!.Title).FullText = _rewrite;

        AppState.NovelInfo.Text = string.Join("\n\n", ChapterOutlines.Select(x => x.FullText));
        AppState.NovelInfo.TextPages.Clear();
    }
}
