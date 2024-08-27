using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReverseMarkdown;

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

	//public static List<NovelChapter> ParseNovelChaptersFromEpub(string base64Data)
	//{
	//	var markdownPath = Path.Combine(RuntimeEpubDirectory, "Markdown");
	//	var chapterList = new List<NovelChapter>();
	//	var files = Directory.GetFiles(markdownPath, "*.md").ToList();
	//	foreach (var file in files)
	//	{
	//		var text = File.ReadAllText(file);
	//		chapterList.Add(new NovelChapter
	//		{
	//			Title = text.Split("\n")[0].Trim(),
	//			Text = text,
	//			Chapter = files.IndexOf(file)
	//		});
	//	}
	//	return chapterList;
	//}
	public static string RuntimeEpubDirectory = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "Epub");
	public static List<NovelChapter> ParseEpubChapters(string epubFileBase64Data, string filePath = "")
	{
		var chapterFileTexts = ExtractEpubFiles(epubFileBase64Data);
		//var runtimeEpubDirectory = Path.Combine(RuntimeEpubDirectory, Path.GetFileNameWithoutExtension(filePath));
		var files = ConvertFilesToMarkdown(chapterFileTexts.ToList());
		//var markdownPath = Path.Combine(runtimeEpubDirectory, "Markdown");
		var chapterList = new List<NovelChapter>();
		//var files = Directory.GetFiles(markdownPath, "*.md").ToList();
		foreach (var file in files)
		{
			var text = file.Item2;
			chapterList.Add(new NovelChapter
			{
				Title = file.Item1 + " " + text.Split("\n")[0].Trim(),
				Text = text,
				Chapter = files.IndexOf(file)
			});
		}
		return chapterList;
	}
	private static List<(string, string)> ConvertFilesToMarkdown(List<(string, string)> files)
	{
		var markdownFiles = new List<(string, string)>();
		foreach (var file in files)
		{
			var htmlStringLines = file.Item2.Split("\n");
			var htmlBuilder = new StringBuilder();
			var isStartText = false;
			var hasTitle = false;
			foreach (var line in htmlStringLines)
			{
				if (isStartText)
				{
					htmlBuilder.AppendLine(line);
				}
				if (line.Contains(@"<h1") && !hasTitle)
				{
					htmlBuilder.AppendLine(line);
					hasTitle = true;
					isStartText = true;
				}
				//else if (line.Contains("<p"))
				//{
				//	isStartText = true;
				//	htmlBuilder.AppendLine(line);
				//}

				//if (line.Contains(@"class=""text"""))
				//	isStartText = true;
			}
			var htmlString = htmlBuilder.ToString();
			var markdown = EpubFileAsMarkdown(htmlString);
			if (string.IsNullOrWhiteSpace(markdown))
				continue;
			markdownFiles.Add((file.Item1, markdown));
		}
		return markdownFiles;
	}
	private static async Task ConvertFilesToMarkdown(string epubPath)
	{

		var files = Directory.GetFiles(epubPath, "*html", SearchOption.AllDirectories)/*.Where(x => x.Contains("c0"))*/;
		var markdownFolder = Path.Combine(epubPath, "Markdown");
		if (!Directory.Exists(markdownFolder))
			Directory.CreateDirectory(markdownFolder);
		foreach (var file in files)
		{
			var htmlStringLines = await File.ReadAllLinesAsync(file);
			var htmlBuilder = new StringBuilder();
			var isStartText = false;
			var hasTitle = false;
			foreach (var line in htmlStringLines)
			{
				if (isStartText)
				{
					htmlBuilder.AppendLine(line);
				}
				else if (line.Contains(@"<h1") && !hasTitle)
				{
					htmlBuilder.AppendLine(line);
					hasTitle = true;
					isStartText = true;
				}
				else if (line.Contains("<p"))
				{
					isStartText = true;
					htmlBuilder.AppendLine(line);
				}

				//if (line.Contains(@"class=""text"""))
				//	isStartText = true;
			}
			var htmlString = htmlBuilder.ToString();
			var markdown = EpubFileAsMarkdown(htmlString);
			if (string.IsNullOrWhiteSpace(markdown))
				continue;
			var markdownFile = Path.Combine(markdownFolder, Path.GetFileNameWithoutExtension(file) + ".md");
			await File.WriteAllTextAsync(markdownFile, markdown);

		}
	}
	private static IEnumerable<(string, string)> ExtractEpubFiles(string epubFileBase64Data)
	{
		//string epubFilePath = @"C:\path\to\your\file.epub";
		//string extractPath = @"C:\path\to\extract\directory";
		//var extractPath = RuntimeEpubDirectory;
		//// Ensure the target directory exists
		//if (!Directory.Exists(extractPath))
		//	Directory.CreateDirectory(extractPath);

		// Open the epub file as a zip archive
		using (var epubStream = ConvertBase64ToMemoryStream(epubFileBase64Data))
		using (var archive = new ZipArchive(epubStream, ZipArchiveMode.Read))
		{
			// Extract all files to the specified directory
			//archive.ExtractToDirectory(extractPath);
			foreach (var entry in archive.Entries)
			{
				if (!entry.FullName.EndsWith("html"))
					continue;
				using var stream = entry.Open();
				yield return (Path.GetFileNameWithoutExtension(entry.FullName), new StreamReader(stream).ReadToEnd());
			}
		}

		Console.WriteLine("Extraction complete.");
	}
	public static MemoryStream ConvertBase64ToMemoryStream(string base64Data)
	{
		var fileBytes = Convert.FromBase64String(base64Data);
		var memoryStream = new MemoryStream(fileBytes);
		memoryStream.Position = 0;
		return memoryStream;
	}
	
	private static string EpubFileAsMarkdown(string htmlString)
	{
		var config = new Config
		{
			// Include the unknown tag completely in the result (default as well)
			UnknownTags = Config.UnknownTagsOption.PassThrough,
			// generate GitHub flavoured markdown, supported for BR, PRE and table tags
			GithubFlavored = false,
			// will ignore all comments
			RemoveComments = true,
			// remove markdown output for links where appropriate
			SmartHrefHandling = true
		};

		var converter = new Converter(config);
		var epubFileAsMarkdown = converter.Convert(htmlString);
		return epubFileAsMarkdown;
	}
}
public class NovelChapter
{
	public int Volume { get; set; }
	public int? Part { get; set; }
	public int Chapter { get; set; }
	public string? Text { get; set; }
	public string? Outline { get; set; }
	public string? Title { get; set; }
}