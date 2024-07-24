using System.Text.Json;
using AINovelWriter.Shared.Models;
using AINovelWriter.Shared.Services;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;
using static AINovelWriter.Shared.Models.EnumHelpers;
using static AINovelWriter.Shared.Services.NovelWriterService;

namespace AINovelWriter.Web.Pages;

public partial class Home
{
	private bool _isBusy;
	private string _text = "";
	private int _step;
	private bool _showOutline;
	RadzenButton _button;
	Popup _popup;
	
	private string Username = "Anonymous User";
	private string Picture = "";
	private bool _isCheat;
	[CascadingParameter]
	private Task<AuthenticationState>? AuthenticationState { get; set; }
	[Inject]
	private CosmosService CosmosService { get; set; } = default!;
	private bool _showBookLink;
	protected override async Task OnInitializedAsync()
	{
		if (AuthenticationState is not null)
		{
			var state = await AuthenticationState;

			var userDataUserName = state?.User?.Identity?.Name;
			Picture = state?.User?.Claims
				.Where(c => c.Type.Equals("picture"))
				.Select(c => c.Value)
				.FirstOrDefault() ?? string.Empty;
			var userProfile = await CosmosService.GetUserProfile(userDataUserName ?? string.Empty);
			if (userProfile is not null)
			{
				AppState.UserData = userProfile;
				AppState.UserData.ImagePath ??= Picture;
			}
			else
			{
				AppState.UserData.UserName = userDataUserName ?? string.Empty;
				AppState.UserData.ImagePath ??= Picture;
			}
			//AppState.UserData.UserName = userDataUserName ?? string.Empty;
			
			
		}
#if DEBUG
		_isCheat = true;
		
#endif
		await base.OnInitializedAsync();
	}
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{

		}
		await base.OnAfterRenderAsync(firstRender);
	}
	private async Task Cheat()
	{
		var tempJson = await File.ReadAllTextAsync("FullCheatNovel.json");
		AppState.NovelInfo = JsonSerializer.Deserialize<NovelInfo>(tempJson)!;
		AppState.NovelInfo.IsComplete = true;
		AppState.NovelOutline.Outline = AppState.NovelInfo.Outline;
	}


}