﻿using Microsoft.SemanticKernel.Text;
using System.Text.Json.Serialization;
using IgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
#pragma warning disable SKEXP0050

namespace AINovelWriter.Shared.Models;

public class NovelInfo
{
	public string id => Guid.NewGuid().ToString();
	public string? User { get; set; }
	public string Title { get; set; } = "";
	public string Text { get; set; } = "";
	public string Outline { get; set; } = "";
	public string ImageUrl { get; set; } = "";
	public string ConceptDescription { get; set; } = "";
	public List<string> TextPages { get; private set; } = [];
	[JsonIgnore]
	[Ignore]
	public string ImgHtml => $"<img src='{ImageUrl}' />";
	public List<OutlineChapter> ChapterOutlines { get; set; } = [];
	public void SplitIntoPagesByWords(int wordPerPage = 200)
	{
		TextPages = StringHelpers.SplitStringIntoPagesByWords(Text, wordPerPage);
	}
	public void SplitIntoPagesByLines(int linesPerPage = 20)
	{
		TextPages = StringHelpers.SplitStringIntoPagesByLines(Text, linesPerPage);
	}
	public void SplitIntoPagesByTokenLines(int tokensPerLine = 40, bool is4o = false)
	{
		TextChunker.TokenCounter delegateTokenCounter = is4o ? StringHelpers.GetTokens200K : StringHelpers.GetTokens;
		var lines = TextChunker.SplitPlainTextLines(Text, tokensPerLine, delegateTokenCounter);
		var lines2 = Text.Split("\n\n");
		Console.WriteLine("Lines:");
		//lines.ForEach(Console.WriteLine);
		var chunks = TextChunker.SplitPlainTextParagraphs(lines, 320, tokenCounter:delegateTokenCounter);
		var chunkSizes = chunks.Select(x => delegateTokenCounter(x)).ToList();
		for (var index = 0; index < chunkSizes.Count; index++)
		{
			var chunk = chunkSizes[index];
			Console.WriteLine($"Chunk size, page {index + 1}: {chunk}");
		}

		TextPages = chunks;
		//var result = new List<string>();
		//for (var i = 0; i < lines.Count; i += linesPerPage)
		//{
		//	result.Add(string.Join(Environment.NewLine, lines.GetRange(i, Math.Min(linesPerPage, lines.Count - i))));
		//}
		//TextPages = result;
	}
	[JsonIgnore]
	public bool IsComplete { get; set; }
 
}
public record OutlineChapter(string Title, string Text)
{
	public string? FullText { get; set; }
	public bool ShowAudio { get; set; }
}