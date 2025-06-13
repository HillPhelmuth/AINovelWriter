using AINovelWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Radzen;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;
using static AINovelWriter.Shared.Models.FileHelper;


namespace AINovelWriter.Web.Pages;

public partial class NovelStreamingPage
{
	private Popup? _popup;
	private RadzenButton? _button;
	private CancellationTokenSource _cancellationTokenSource = new();
	
	private bool _isCheat;
	private List<string> _pages = [];
	
    private string _buttonClass = "";
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && string.IsNullOrEmpty(AppState.NovelInfo.Text))
		{
			await GenerateNovelAsync();
		}
		await base.OnAfterRenderAsync(firstRender);
	}

	public async Task GenerateNovelAsync()
	{
		if (string.IsNullOrEmpty(AppState.NovelOutline.Outline)) return;
		AppState.NovelInfo.Text = "";
		IsBusy = true;
		StateHasChanged();
		var ctoken = _cancellationTokenSource.Token;
        try
        {
            await foreach (var token in NovelWriterService.WriteFullNovel(AppState.NovelOutline.Outline!, AppState.NovelOutline.WriterAIModel, ctoken))
            {
                AppState.NovelInfo.Text += token;
                await InvokeAsync(StateHasChanged);
            }
        }
        catch
        {
            NotificationService.Notify(NotificationSeverity.Error, "Novel Generation Failed",
                "Novel generation was cancelled or failed.");
        }
        finally
        {
            AppState.NovelInfo.IsComplete = true;
            IsBusy = false;
            _buttonClass = "blink_me";
            StateHasChanged();
        }

    }

    private void Cancel()
	{
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
        AppState.NovelInfo.IsComplete = true;
        IsBusy = false;
        _buttonClass = "blink_me";
        StateHasChanged();
    }
}