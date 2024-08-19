using System.Text.Json;
using AINovelWriter.Shared.Models;

namespace AINovelWriter.Web.Pages;

public partial class Home
{
    private bool _isCheat;

    protected override async Task OnInitializedAsync()
	{
		
#if DEBUG
		_isCheat = true;
#endif
		await base.OnInitializedAsync();
	}
	
	private async Task Cheat()
	{
		var tempJson = await File.ReadAllTextAsync("FullCheatNovel.json");
		AppState.NovelInfo = JsonSerializer.Deserialize<NovelInfo>(tempJson)!;
		AppState.NovelInfo.IsComplete = true;
		AppState.NovelOutline.Outline = AppState.NovelInfo.Outline;
	}


}