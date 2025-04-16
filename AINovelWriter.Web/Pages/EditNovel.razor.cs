using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static AINovelWriter.Shared.Models.EnumHelpers;

namespace AINovelWriter.Web.Pages;
public partial class EditNovel
{
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    private List<ChapterOutline> ChapterOutlines => AppState.NovelInfo.ChapterOutlines;
    
    private static Dictionary<AIModel, string> AIModelDescriptions => GetEnumsWithDescriptions<AIModel>().ToDictionary(x => x.Key, y => y.Value);
    private static Dictionary<ReviewContext, string> ReviewContextDescriptions => GetEnumsWithDescriptions<ReviewContext>().ToDictionary(x => x.Key, y => y.Value);

    

    public class EditNovelForm
    {
        public ChapterOutline? ChapterOutline { get; set; }
        public string? Notes { get; set; }
        public AIModel AIModel { get; set; }
    }

    public class ReviewNovelForm
    {
        public AIModel AIModel { get; set; }
        public ReviewContext ReviewContext { get; set; }
    }
   
    private EditNovelForm _editNovelForm = new();
    private Feedback? _feedback;
    public class ApplySuggestionForm
    {
        public Feedback? Feedback { get; set; }
        public ChapterOutline? ChapterOutline { get; set; }
        public AIModel AIModel { get; set; }
        public string? AdditionalNotes { get; set; }
    }
    private ApplySuggestionForm _applySuggestionForm = new();
    private bool _isBusy;
    private string _originalText = "";
    private string _fullReviewText = "";
    private ReviewNovelForm _reviewNovelForm = new();

    [Parameter]
    public int SelectedTabIndex { get; set; }

    protected override Task OnParametersSetAsync()
    {
        if (SelectedTabIndex > 0)
        {
            StateHasChanged();
        }
        return base.OnParametersSetAsync();
    }

    private async Task FullReview(ReviewNovelForm form)
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var model = form.AIModel;
        _fullReviewText = "";
        await foreach (var review in NovelWriterService.ReviewFullNovel(AppState.NovelInfo, form.ReviewContext, model))
		{
			_fullReviewText += review;
			await InvokeAsync(StateHasChanged);
		}
        _isBusy = false;
        StateHasChanged();
    }
    public async void GetFeedback(EditNovelForm form)
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var text = form.ChapterOutline?.FullText;
        _originalText = text;
        var feedback = await NovelWriterService.ProvideRewriteFeedback(form.ChapterOutline, form.AIModel, form.Notes);
        _applySuggestionForm.Feedback = feedback;
        _applySuggestionForm.ChapterOutline = form.ChapterOutline;
        _isBusy = false;
        StateHasChanged();
    }
    private string _rewrite = "";
    private bool _isRewriting;
    public async Task ApplySuggestions(ApplySuggestionForm form)
    {
        _isRewriting = true;
        StateHasChanged();
        await Task.Delay(1);
        var chapterRewrite = await NovelWriterService.RewriteChapter(form.ChapterOutline!, form.Feedback!, form.AIModel, form.AdditionalNotes);
        _rewrite = chapterRewrite;
        //AppState.NovelInfo.ChapterOutlines.First(x => x.Title == form.ChapterOutline.Title).FullText = chapterRewrite;

        //AppState.NovelInfo.Text = string.Join("\n\n", ChapterOutlines.Select(x => x.FullText));
        _isRewriting = false;
        StateHasChanged();
    }
    private AIModel _compareModel;
    public async Task CompareRewrite(AIModel model)
    {
        var versionA = _editNovelForm.ChapterOutline!.FullText;
        var versionB = _rewrite;
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var diff = await NovelWriterService.CompareTwoChapterVersions(versionA, versionB, model);
        _isBusy = false;
        StateHasChanged();
        await ShowInlineDialog(diff);
    }

    private void ApplyRewrite()
    {
        AppState.NovelInfo.ChapterOutlines.First(x => x.Title == _applySuggestionForm.ChapterOutline!.Title).FullText = _rewrite;

        AppState.NovelInfo.Text = string.Join("\n\n", ChapterOutlines.Select(x => x.FullText));
        AppState.NovelInfo.TextPages.Clear();
    }
    
    public class ChapterTextEdit(string orignalTitle, string originalText, string outlineText)
    {
        public string OriginalText { get; } = originalText;
        public string OriginalTitle { get; } = orignalTitle;
        public string NewTitle { get; set; } = orignalTitle;
        public string NewText { get; set; } = originalText;
        public string OutlineText { get; } = outlineText;
    }

    private ChapterTextEdit? _chapterTextEdit;
    private void EditChapter(ChapterOutline chapterOutline)
    {
        _chapterTextEdit = new ChapterTextEdit(chapterOutline.Title, chapterOutline.FullText, chapterOutline.Text);
        StateHasChanged();
    }
    private async void SubmitEdit(ChapterTextEdit chapterTextEdit)
    {
        var confirm = await DialogService.Confirm("Are you sure you want to save these changes?", "Save Changes");
        if (confirm != true) return;
        var originalChapter = AppState.NovelInfo.ChapterOutlines.First(x => x.Title == chapterTextEdit.OriginalTitle);
        var newChapter = new ChapterOutline(chapterTextEdit.NewTitle, chapterTextEdit.OutlineText, originalChapter.ChapterNumber){FullText = chapterTextEdit.NewText};
        AppState.NovelInfo.ChapterOutlines[AppState.NovelInfo.ChapterOutlines.IndexOf(originalChapter)] = newChapter;
        AppState.NovelInfo.Text = string.Join("\n\n", ChapterOutlines.Select(x => x.FullText));
        AppState.NovelInfo.TextPages.Clear();
        AppState.NovelInfo.Outline = string.Join("\n\n", ChapterOutlines.Select(x => x.Text));
        AppState.NovelOutline.Outline = AppState.NovelInfo.Outline;
        _chapterTextEdit = null;
        StateHasChanged();
    }

    private async void CompareEdit()
    {
        var original = _chapterTextEdit!.OriginalText;
        var newText = _chapterTextEdit!.NewText;
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var diff = await NovelWriterService.CompareTwoChapterVersions(original, newText, _compareModel);
        _isBusy = false;
        StateHasChanged();
        await ShowInlineDialog(diff);
    }
    private async Task DeleteChapter(ChapterOutline chapterOutline)
    {
        var confirm = await DialogService.Confirm("Are you sure you want to delete this chapter?", "Delete Chapter");
        if (confirm != true) return;
        AppState.NovelInfo.ChapterOutlines.Remove(chapterOutline);
        AppState.NovelInfo.Text = string.Join("\n\n", ChapterOutlines.Select(x => x.FullText));
        AppState.NovelInfo.TextPages.Clear();
        AppState.NovelInfo.Outline = string.Join("\n\n", ChapterOutlines.Select(x => x.Text));
        AppState.NovelOutline.Outline = AppState.NovelInfo.Outline;
        StateHasChanged();
    }
}
