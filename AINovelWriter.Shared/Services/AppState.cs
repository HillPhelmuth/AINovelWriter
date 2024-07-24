using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AINovelWriter.Shared.Models;

namespace AINovelWriter.Shared.Services;

public class AppState : INotifyPropertyChanged
{
	private NovelInfo _novelInfo = new();
	private NovelConcepts _novelConcepts = new();
	private UserData _userData = new();
	private NovelOutline _novelOutline = new();
    private string? _currentPage;
    public event PropertyChangedEventHandler? PropertyChanged;
	public AIModel WriterModel { get; set; }
    public string? CurrentPage
    {
        get => _currentPage;
        set => SetField(ref _currentPage, value);
    }

    public NovelInfo NovelInfo
	{
		get => _novelInfo;
		set => SetField(ref _novelInfo, value);
	}

	public NovelConcepts NovelConcepts
	{
		get => _novelConcepts;
		set => SetField(ref _novelConcepts, value);
	}

	public NovelOutline NovelOutline
	{
		get => _novelOutline;
		set => SetField(ref _novelOutline, value);
	}

	public UserData UserData
	{
		get => _userData;
		set => SetField(ref _userData, value);
	}
	public List<string> GeneratedImages { get; set; } = [];
	public void NovelComplete()
	{
		NovelInfo.IsComplete = true;
		OnPropertyChanged(nameof(NovelInfo));
	}
	public void AddCover(string image)
	{
		NovelInfo.ImageUrl = image;
		OnPropertyChanged(nameof(NovelInfo));
	}
	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}