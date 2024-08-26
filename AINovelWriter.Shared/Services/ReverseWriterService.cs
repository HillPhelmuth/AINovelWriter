using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AINovelWriter.Shared.Services;

public class ReverseWriterService
{
	public static List<NovelChapter> ParseNovelChapters(string directoryPath)
	{
		var chapterList = new List<NovelChapter>();
		var files = Directory.GetFiles(directoryPath, "*.md");

		var regex = new Regex(@"vol-(\d{3})-(part-(\d{3})-)?chapter-(\d{3})\.md", RegexOptions.IgnoreCase);

		foreach (var file in files)
		{
			var fileName = Path.GetFileName(file);
			var match = regex.Match(fileName);
			var text = File.ReadAllText(file);
			if (match.Success)
			{
				if (int.TryParse(match.Groups[1].Value, out int volume) &&
				    int.TryParse(match.Groups[4].Value, out int chapter))
				{
					int? part = null;
					if (match.Groups[3].Success && int.TryParse(match.Groups[3].Value, out int partValue))
					{
						part = partValue;
					}

					chapterList.Add(new NovelChapter
					{
						Volume = volume,
						Part = part,
						Chapter = chapter,
						Text = text
					});
				}
			}
		}

		return chapterList;
	}
}
public class NovelChapter
{
	public int Volume { get; set; }
	public int? Part { get; set; }
	public int Chapter { get; set; }
	public string? Text { get; set; }
	public string? Outline { get; set; }
}