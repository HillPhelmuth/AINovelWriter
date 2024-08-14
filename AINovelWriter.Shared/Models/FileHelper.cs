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

namespace AINovelWriter.Shared.Models;

public class FileHelper
{
    public static byte[] GenerateTextFile(string content)
    {
        return Encoding.UTF8.GetBytes(content);
    }
    public static byte[] CreatePdf(Stream coverImageStream, string novelText)
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

        // Create a new MigraDoc document
        var doc = new Document();
        var section = doc.AddSection();

        // Read the novel text from the file
        //var novelText = File.ReadAllText(novelText);

        // Add the novel text to the section
        section.AddParagraph(novelText);

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

        // Append the novel text pages to the PDF document
        //foreach (var page in renderer.PdfDocument.Pages)
        //{
        //    pdfDocument.AddPage(page);
        //}

        //// Save the PDF document to the specified path
        //using var stream = new MemoryStream();
        //pdfDocument.Save(stream, false);
        //return stream.ToArray();
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
}