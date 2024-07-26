﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AINovelWriter.Shared.Models;

public class FileHelper
{
    public static byte[] GenerateTextFile(string content)
    {
        return Encoding.UTF8.GetBytes(content);
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