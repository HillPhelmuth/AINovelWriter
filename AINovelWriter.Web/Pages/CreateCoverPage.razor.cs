using AINovelWriter.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using AINovelWriter.Shared.Services;


namespace AINovelWriter.Web.Pages;

public partial class CreateCoverPage
{
	[Inject]
	private ImageGenService ImageGenService { get; set; } = default!;
	private class ImageForm
	{
		public List<CoverArtStyle> CoverArtStyles { get; set; } = [];
	}
	private ImageForm _imageForm = new();
	private class ArtStyleInfo(CoverArtStyle coverArtStyle, string displayname, string description)
	{
		public CoverArtStyle CoverArtStyle { get; set; } = coverArtStyle;
		public string DisplayName { get; set; } = displayname;
		public string Description { get; set; } = description;
	}

	private Dictionary<CoverArtStyle, string> CoverArtStyleDescriptions => EnumHelpers.GetEnumsWithDescriptions<CoverArtStyle>();
	private List<ArtStyleInfo> ArtStyleNamesAndDescriptions => EnumHelpers.GetEnumsWithDisplayAndDescriptions<CoverArtStyle>().Select(x => new ArtStyleInfo(x.Item1, x.Item2, x.Item3)).ToList();
	private string[] _images = [];
	private bool _isBusy;

	protected override Task OnInitializedAsync()
	{
		if (!AppState.NovelInfo.IsComplete) NavigationManager.NavigateTo("");
		return base.OnInitializedAsync();
	}

	private async void GenerateImage(ImageForm imageForm)
	{
		_isBusy = true;
		StateHasChanged();
		await Task.Delay(1);
		var tasks = new List<Task<string>>();
		foreach (var style in imageForm.CoverArtStyles)
		{
			var isVivid = style is CoverArtStyle.PhotoRealistic or CoverArtStyle.SciFiArt or CoverArtStyle.FantasyArt;
			var imageTask = ImageGenService.GenerateImage(AppState.NovelInfo, $"{style.GetDisplayName()} - {style.GetDescription()}", isVivid);
			tasks.Add(imageTask);

		}
		var images = await Task.WhenAll(tasks);
		AppState.GeneratedImages = [.. images];
		_isBusy = false;
		StateHasChanged();
	}
	private async void SelectImage(string image)
	{
		if (_isBusy) return;
		var imageUrl = await ImageGenService.SelectImage(AppState.NovelInfo, image);
		AppState.AddCover(imageUrl);
		NavigationManager.NavigateTo("book");
	}
}