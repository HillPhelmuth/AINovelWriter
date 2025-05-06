using AINovelWriter.Shared.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace AINovelWriter.Web.Components;
public partial class ShareNovel
{
    [Parameter]
    public SharedNovelInfo? SharedNovelInfo { get; set; }

    private async Task Share()
    {
        if (SharedNovelInfo is null || string.IsNullOrEmpty(SharedNovelInfo.SharedBy))
        {
            Console.WriteLine("SharedNovelInfo is null");
            return;
        }
        try
        {
            var result = await CosmosService.ShareNovel(SharedNovelInfo);
            if (result.Item1)
            {
                NotificationService.Notify(NotificationSeverity.Success, "Novel shared successfully.", result.Item2);
                if (AppState.UserData.SavedNovels.Any(x => x.NovelId == SharedNovelInfo.id))
                {
                    AppState.UserData.SavedNovels.Find(x => x.NovelId == SharedNovelInfo.id)!.IsShared = true;
                    await CosmosService.SaveUser(AppState.UserData);
                }
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Failed to share novel.", result.Item2);
            }
            DialogService.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sharing novel: {ex.Message}");
            NotificationService.Notify(NotificationSeverity.Error, "Failed to share novel.", ex.Message);
            DialogService.Close();
        }

    }

    private void Cancel()
    {
        DialogService.Close();
    }
}
