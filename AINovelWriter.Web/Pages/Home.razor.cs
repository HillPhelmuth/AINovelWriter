﻿using System.Text.Json;
using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;

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
	private bool _isBusy;
    private async Task Cheat()
	{
		//      _isBusy = true;
		//      StateHasChanged();
		//await Task.Delay(1);
		//      AppState.NovelInfo =
		//	await NovelWriterService.ReverseEngineerNovel(
		//		@"C:\Users\adamh\OneDrive\Documents\Novel Files\Sword_Magic\Markdown",3, "The Last Priestess: The Songmaker Book 1");
		var tempJson = await File.ReadAllTextAsync("FullCheatNovel.json");
		AppState.NovelInfo = JsonSerializer.Deserialize<NovelInfo>(tempJson)!;
		//await File.WriteAllTextAsync("Sword_Magic_3.json", JsonSerializer.Serialize(AppState.NovelInfo));
		AppState.NovelInfo.IsComplete = true;
		AppState.NovelOutline.Outline = AppState.NovelInfo.Outline;
		_isBusy = false;
        StateHasChanged();
    }


}