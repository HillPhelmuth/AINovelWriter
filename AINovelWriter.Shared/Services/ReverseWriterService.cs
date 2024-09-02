using System.ComponentModel;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;
using ReverseMarkdown;

namespace AINovelWriter.Shared.Services;

public class ReverseWriterService
{
    public static List<NovelChapter> ParseEpubChapters(string epubFileBase64Data, string filePath = "")
    {
        var chapterFileTexts = ExtractEpubFiles(epubFileBase64Data);
        var markdownFiles = ConvertFilesToMarkdown(chapterFileTexts.ToList());
        var chapterList = new List<NovelChapter>();
        foreach (var file in markdownFiles)
        {
            var text = file.Item2;
            chapterList.Add(new NovelChapter
            {
                Title = file.Item1 + " " + text.Split("\n")[0].Trim(),
                Text = text,
                Chapter = markdownFiles.IndexOf(file)
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
            markdownFiles.Add((file.Item1, markdown));
        }
        return markdownFiles;
    }
    private static async Task ConvertFilesToMarkdown(string epubPath)
    {

        var files = Directory.GetFiles(epubPath, "*html", SearchOption.AllDirectories);
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
       
        // Open the epub file as a zip archive
        using (var epubStream = ConvertBase64ToMemoryStream(epubFileBase64Data))
        using (var archive = new ZipArchive(epubStream, ZipArchiveMode.Read))
        {
            // Extract all files to the specified directory
            foreach (var entry in archive.Entries)
            {
               
                if (!entry.FullName.EndsWith("html") && !entry.FullName.EndsWith("xml"))
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
