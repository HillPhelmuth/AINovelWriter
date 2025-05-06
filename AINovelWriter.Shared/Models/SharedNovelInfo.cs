namespace AINovelWriter.Shared.Models;

public class SharedNovelInfo
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public DateOnly CreatedOn { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly SharedOn { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? SharedBy { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public NovelTone Tone { get; set; } = NovelTone.None;
    public NovelAudience Audience { get; set; } = NovelAudience.None;
    public List<string> TextPages { get; private set; } = [];
    public static SharedNovelInfo FromNovelInfo(NovelInfo novelInfo)
    {
        return new SharedNovelInfo
        {
            id = novelInfo.id,
            CreatedOn = novelInfo.CreatedOn,
            SharedBy = novelInfo.User,
            Title = novelInfo.Title,
            Text = novelInfo.Text,
            ImageUrl = novelInfo.ImageUrl,
            TextPages = novelInfo.TextPages,
            Tone = novelInfo.Concepts?.Tone ?? NovelTone.None,
            Audience = novelInfo.Concepts?.Audience ?? NovelAudience.None
        };
    }
    public NovelInfo ToNovelInfo(string currentUser)
    {
        var novelInfo = new NovelInfo
        {
            id = id,
            CreatedOn = CreatedOn,
            User = currentUser,
            Title = Title,
            Text = Text,
            ImageUrl = ImageUrl
            
        };
        novelInfo.SplitIntoPagesByTokenLines(is4o:true);
        return novelInfo;
    }
}