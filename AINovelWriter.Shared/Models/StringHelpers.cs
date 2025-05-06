using System.Text.RegularExpressions;
using Tiktoken;
using Encoder = Tiktoken.Encoder;

namespace AINovelWriter.Shared.Models;

public static class StringHelpers
{
	public static List<string> SplitText(this string text, int maxCharLength)
	{
		var result = new List<string>();

		if (string.IsNullOrWhiteSpace(text))
		{
			return result;
		}

		var length = text.Length;
		var start = 0;

		while (start < length)
		{
			var end = start + maxCharLength;

			if (end >= length)
			{
				result.Add(text[start..]);
				break;
			}

			var lastSpace = text.LastIndexOf(' ', end, end - start);

			if (lastSpace > start)
			{
				result.Add(text.Substring(start, lastSpace - start));
				start = lastSpace + 1;
			}
			else
			{
				result.Add(text.Substring(start, maxCharLength));
				start += maxCharLength;
			}
		}

		return result;
	}
	public static List<string> SplitStringIntoPagesByWords(string input, int wordsPerPage = 450)
	{
		var chunks = new List<string>();
		if (string.IsNullOrWhiteSpace(input) || wordsPerPage <= 0)
		{
			return chunks;
		}

		var words = input.Split([' ', '\t', '\n', '\r']);
		var wordCount = words.Length;
		var currentWordIndex = 0;

		while (currentWordIndex < wordCount)
		{
			var endWordIndex = Math.Min(currentWordIndex + wordsPerPage, wordCount);
			var chunk = string.Join(" ", words.Skip(currentWordIndex).Take(endWordIndex - currentWordIndex));
			chunks.Add(chunk);
			currentWordIndex = endWordIndex;
		}

		return chunks;
	}
	public static List<string> SplitStringIntoPagesByLines(string input, int linesPerSegment = 20)
	{
		var result = new List<string>();
		var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		for (int i = 0; i < lines.Length; i += linesPerSegment)
		{
			result.Add(string.Join(Environment.NewLine, lines, i, Math.Min(linesPerSegment, lines.Length - i)));
		}
		return result;
	}
	private static Encoder _encoding100K = ModelToEncoder.For("gpt-4");
	private static Encoder _encoder200K = ModelToEncoder.For("gpt-4o");
	public static int GetTokens(string text)
	{
		return _encoding100K.CountTokens(text);
	}
	public static int GetTokens200K(string text)
	{
		return _encoder200K.CountTokens(text);
	}
	public static string RemoveAllEscaping(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}

		// Pattern to match escape sequences including unicode and control characters
		string pattern = @"\\[abfnrtv\\'""0-9xu]{1,6}";
		return Regex.Replace(input, pattern, "").Replace("\n", " ");
	}
	public static string ReplaceChapter(string originalText, string chapterTitle, string newChapterText)
	{
		// Find the start of the chapter to replace
		var startIndex = originalText.IndexOf(chapterTitle, StringComparison.Ordinal);
		if (startIndex == -1)
		{
			throw new ArgumentException("Chapter title not found in the original text.");
		}

		// Find the start of the next chapter to determine the end of the current chapter
		var endIndex = originalText.IndexOf("## Chapter", startIndex + chapterTitle.Length, StringComparison.Ordinal);

		if (endIndex == -1)
		{
			// If this is the last chapter, we replace everything till the end of the text
			endIndex = originalText.Length;
		}

		// Extract the text before and after the chapter
		var beforeChapter = originalText[..startIndex];
		var afterChapter = originalText[endIndex..];

		// Combine the text before, the new chapter text, and the text after
		var updatedText = beforeChapter + newChapterText + afterChapter;

		return updatedText;
	}
    public static List<string> SplitMarkdownByHeaders(string markdownText)
    {
        var result = new List<string>();
        var headerPattern = new Regex(@"^(## .+)$", RegexOptions.Multiline);

        var matches = headerPattern.Matches(markdownText);

        var lastIndex = 0;
        foreach (Match match in matches)
        {
            if (lastIndex != match.Index)
            {
                if (lastIndex != 0)
                {
                    var segment = markdownText.Substring(lastIndex, match.Index - lastIndex).Trim();
                    result.Add(segment);
                }
                else
                {
                    // Capture the first header and content if the first header is at the start
                    var firstSegment = markdownText[..match.Index].Trim();
                    if (!string.IsNullOrEmpty(firstSegment))
                    {
                        result.Add(firstSegment);
                    }
                }
            }
            lastIndex = match.Index;
        }

        if (lastIndex == 0) return result;
        var lastSegment = markdownText[lastIndex..].Trim();
        result.Add(lastSegment);

        return result;
    }
}