using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Pdf.IO;
using System.Security;
using Markdig;

namespace AINovelWriter.Shared.Models;

public class FileHelper
{
    public static byte[] GenerateTextFile(string content)
    {
        return Encoding.UTF8.GetBytes(content);
    }
    public static byte[] CreatePdf(Stream coverImageStream, List<string> chapterTexts, string coverTitle = "",
	    string? novelText = null)
    {
        // Create a new PDF document
        var pdfDocument = new PdfDocument();

        // Add a cover page
        var coverPage = pdfDocument.AddPage();
        var gfx = XGraphics.FromPdfPage(coverPage);
        //var coverImageStream = new MemoryStream(coverImageData,0, coverImageData.Length,false,true);
        var coverImage = XImage.FromStream(coverImageStream);

        // Draw the cover image on the cover page
        gfx.DrawImage(coverImage, 0, 0, coverPage.Width, coverPage.Height);
        // Draw the title text on top of the cover image
        var titleFont = new XFont("Arial", 36, XFontStyleEx.Bold);
        XBrush textBrush = XBrushes.White;
        // Define the area where the title will be drawn (cover page width with some padding)
        double margin = 60; // Adjust margin as needed
        double maxWidth = coverPage.Width - 2 * margin;

        // Calculate the height of each line
        double lineHeight = gfx.MeasureString("A", titleFont).Height;

        // Break the title into lines that fit within the maxWidth
        List<string> titleLines = new List<string>();
        string[] words = coverTitle.Split(' ');
        string currentLine = "";

        foreach (string word in words)
        {
	        string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
	        double testWidth = gfx.MeasureString(testLine, titleFont).Width;

	        if (testWidth < maxWidth)
	        {
		        currentLine = testLine;
	        }
	        else
	        {
		        titleLines.Add(currentLine);
		        currentLine = word;
	        }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
	        titleLines.Add(currentLine);
        }

        // Calculate the starting Y position to center the text block
        double totalHeight = titleLines.Count * lineHeight;
        double startY = (coverPage.Height - totalHeight) / 4;
        titleLines.Clear();
        // Draw each line centered
        for (int i = 0; i < titleLines.Count; i++)
        {
	        double lineWidth = gfx.MeasureString(titleLines[i], titleFont).Width;
	        double startX = (coverPage.Width - lineWidth) / 2;
	        gfx.DrawString(titleLines[i], titleFont, textBrush, new XRect(startX, startY + i * lineHeight, lineWidth, lineHeight), XStringFormats.TopLeft);
        }
		// Measure the title size
		

		// Create a new MigraDoc document
		var doc = new Document();
        foreach (var chapter in chapterTexts)
        {
            // Split the chapter into title and content
            var chapterLines = chapter.Split(['\n', '\r']);
            if (chapterLines.Length <= 0) continue;
            var title = chapterLines[0].TrimStart('#').Trim();
            var content = string.Join("\n", chapterLines, 1, chapterLines.Length - 1);

            // Add a new section for each chapter
            var section = doc.AddSection();

            // Add the chapter title
            var titleParagraph = section.AddParagraph(title);
            titleParagraph.Format.Font.Size = 20;
            titleParagraph.Format.Font.Bold = true;
            titleParagraph.Format.Alignment = ParagraphAlignment.Center;

            // Add the chapter content
            section.AddParagraph(content);

        }
       

        // Render the MigraDoc document to the PdfDocument
        var renderer = new PdfDocumentRenderer
        {
            Document = doc
        };
        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        var tempDoc = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

        foreach (var page in tempDoc.Pages)
        {
            pdfDocument.AddPage(page);
        }

        // Save the final PdfDocument to another MemoryStream
        using var finalStream = new MemoryStream();
        pdfDocument.Save(finalStream, false);
        return finalStream.ToArray();

        
    }
    public static async Task<byte[]> CreateAndCompressFilesAsync(string text, string? imageUrl = null)
    {
	    // Create temporary file paths
	    var txtFilePath = Path.GetTempFileName();
	    var imageFilePath = Path.GetTempFileName();
	    var zipFilePath = Path.GetTempFileName();

	    try
	    {
		    // Create a .txt file from the provided string
		    await File.WriteAllTextAsync(txtFilePath, text);

		    // Download the image from the URL and save it as a file
		    if (!string.IsNullOrEmpty(imageUrl))
		    {
			    using var client = new HttpClient();
			    var imageBytes = await client.GetByteArrayAsync(imageUrl);
			    await File.WriteAllBytesAsync(imageFilePath, imageBytes);
		    }

		    // Compress the .txt file and the image file into a .zip file
		    await using (var zipToOpen = new FileStream(zipFilePath, FileMode.Create))
		    using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
		    {
			    archive.CreateEntryFromFile(txtFilePath, "bookText.txt");
			    if (!string.IsNullOrEmpty(imageUrl))
				    archive.CreateEntryFromFile(imageFilePath, "cover.png");
		    }

		    // Read the zip file as byte array
		    var zipBytes = await File.ReadAllBytesAsync(zipFilePath);
		    return zipBytes;
	    }
	    finally
	    {
		    // Clean up the temporary files
		    File.Delete(txtFilePath);
		    if (!string.IsNullOrEmpty(imageUrl))
			    File.Delete(imageFilePath);
		    File.Delete(zipFilePath);
		    Console.WriteLine("Files Deleted");
	    }
    }
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public static byte[] CreateEpubFromNovel(NovelInfo novel, Stream coverImageStream)
    {
        using var outputStream = new MemoryStream();
        using (var zip = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            // 1. mimetype
            var mimeEntry = zip.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (var mimeStream = mimeEntry.Open())
            {
                var bytes = "application/epub+zip"u8.ToArray();
                mimeStream.Write(bytes, 0, bytes.Length);
            }

            // 2. META-INF/container.xml
            AddTextFile(zip, "META-INF/container.xml", Utf8NoBom,
                """
                <?xml version="1.0"?>
                <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
                  <rootfiles>
                    <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
                  </rootfiles>
                </container>
                """);

            // Build manifest + spine for chapters
            var manifest = new StringBuilder();
            var spine = new StringBuilder();
            for (var i = 0; i < novel.ChapterOutlines.Count; i++)
            {
                string id = $"chap{i + 1}";
                manifest.AppendLine($"""    <item id="{id}" href="chapter{i + 1}.xhtml" media-type="application/xhtml+xml"/>""");
                spine.AppendLine($"""    <itemref idref="{id}"/>""");
            }

            // 3. OEBPS/content.opf (with cover)
            AddTextFile(zip, "OEBPS/content.opf", Utf8NoBom,
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                 <package xmlns="http://www.idpf.org/2007/opf" unique-identifier="BookId" version="3.0">
                   <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                     <dc:identifier id="BookId">urn:uuid:{Guid.NewGuid()}</dc:identifier>
                     <dc:title>{SecurityElement.Escape(novel.Title)}</dc:title>
                     <dc:creator>{SecurityElement.Escape(novel.User)}</dc:creator>
                     <dc:language>en</dc:language>
                     <meta name="cover" content="cover-image"/>
                   </metadata>
                   <manifest>
                     <item id="cover-image" href="images/cover.jpg" media-type="image/jpeg" properties="cover-image"/>
                     <item id="nav" href="nav.xhtml" media-type="application/xhtml+xml" properties="nav"/>
                 {manifest.ToString().TrimEnd()}
                   </manifest>
                   <spine>
                 {spine.ToString().TrimEnd()}
                   </spine>
                 </package>
                 """);

            // 4. OEBPS/nav.xhtml
            var navBody = new StringBuilder();
            navBody.AppendLine("""
                               <?xml version="1.0" encoding="utf-8"?>
                               <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
                                 <head><title>Table of Contents</title></head>
                                 <body>
                                   <nav epub:type="toc">
                                     <ol>
                               """);
            for (int i = 0; i < novel.ChapterOutlines.Count; i++)
                navBody.AppendLine($"""        <li><a href="chapter{i + 1}.xhtml">Chapter {i + 1}</a></li>""");
            navBody.Append(
                """
                      </ol>
                    </nav>
                  </body>
                </html>
                """);
            AddTextFile(zip, "OEBPS/nav.xhtml", Utf8NoBom, navBody.ToString());

            // 5. chapters
            for (int i = 0; i < novel.ChapterOutlines.Count; i++)
            {
                var fullTextHtml = AsHtml(novel.ChapterOutlines[i].FullText);
                AddTextFile(zip, $"OEBPS/chapter{i + 1}.xhtml", Utf8NoBom,
                    $"""
                     <?xml version="1.0" encoding="utf-8"?>
                     <html xmlns="http://www.w3.org/1999/xhtml">
                       <head><title>Chapter {i + 1}</title></head>
                       <body>{fullTextHtml}</body>
                     </html>
                     """);
            }

            // 6. cover image
            coverImageStream.Position = 0;
            var coverEntry = zip.CreateEntry("OEBPS/images/cover.jpg", CompressionLevel.Optimal);
            using var coverStream = coverEntry.Open();
            coverImageStream.CopyTo(coverStream);
        }

        return outputStream.ToArray();
    }

    // Helper to add UTF‑8 text files without BOM unless requested.
    private static void AddTextFile(ZipArchive zip, string path, Encoding encoding, string content)
    {
        var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), encoding);
        writer.Write(content);
    }
    private static string AsHtml(string? text)
    {
        if (text == null) return "";
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build(); ;
        var result = Markdown.ToHtml(text, pipeline);
        return result;

    }
}