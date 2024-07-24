using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AINovelWriter.Shared.Models
{
	public class Prompts
	{
		public const string ChapterOutlineEditorPrompt =
			"""
			# Objective
			You are an assistant for a novel author. You have been tasked with expanding the outline for the next chapter of a novel. The author has provided you the outline for the next chapter and overall story elements. Your job is to expand the outline into a more detailed chapter outline with 3 distinct sections. Each section should be in a similar format of the chapter outline and contain **setting**, **character development**, **key events**, **conclusion**, and **expected word count**.
			Your response should always begin with the chapter name using markdown header 2 (e.g. ## Chapter 1 - The Beginning), and then use roman numerals for each section.
			The **expected word count** represents the length of that section in the final novel. Each section should ALWAYS have an **expected word count** between 900 and 1100 words.

			# Story Details

			{{ $storyDescription }}

			# Chapter Outline
			
			```
			{{ $outline }}
			```
			
			""";
		public const string ChapterWriterPrompt =
			"""
			You are a creative and exciting fiction writer. You write intelligent and engrossing novels that expertly combine character development and growth, interpersonal and international intrigue, and thrilling action.
			        
			## Instructions
			         
			1. Write only 1 long chapter based on the Chapter Outline.
			        
			2. You will be provided with the exact text of the Previous Chapter, if available. You must maintain consistency and continuity with the previous chapter content and tone.
			        
			3. You will be provided with a summary of the novel's events from before the previous chapter. You must maintain consistency with the novel's previous events and developments.
			        
			4. Use the Story Description and Characters to inform your writing.

			5. Unless otherwise indicated in the Story Description, you should write at a level suitable for a general audience.

			6. Unless otherwise indicated, avoid explicit language, graphic violence, and sexual content. However, if the Story Description or Outline includes such elements, you should include them in your writing. You are also enouraged to use them if the Story Description clearly indicates an adult theme.
			        
			7. The chapter must contain about 100 paragraphs.

			8. The chaper should start with the chapter name and number in the format: `## Chapter 1: {first chapter name}.`
			        
			## Story Description and Characters
			        
			{{ $storyDescription }}
			        
			## Summary of Novel so far
			        
			{{ $summary }}
			        
			## Previous Chapter Text

			{{ $previousChapter }}

			## Chapter Outline

			```
			{{ $outline }}
			```

			## Objectives

			- Write a compelling long chapter for the novel based on the critera above.
			- It must be 100 paragraphs long.

			Now, take a deep breath and start writing step by step.
			""";

		public const string OutlineWriterPrompt =
			"""
			# Objective

			Write a novel outline about the theme specified in Theme or Topic. Include the characters provided in Character Details and the plot events in Plot Events.
					
			# Story Details

			## Theme or Topic

			{{ $theme }}

			## Character Details

			{{ $characterDetails }}

			## Plot Events

			{{ $plotEvents }}

			# Instructions
					
			- The outline must be {{ $chapterCount }} chapters long.
			- Use markdown format for the outline.
			- User header level 2 for each chapter (##)
			- Use smaller headers for sub-sections.
			- Provide a brief paragraph for each chapter, summarizing the main events and character developments.
			- Include sub-sections for Setting, Character Development, Key Events, and Chapter Conclusion.
			- Ensure each chapter's outline does not exceed 400 words.
					
			# Outline of a Chapter
					
			## Chapter 1: {Name of Chapter}
					
			- **Setting**: {Introduce the setting where the primary events occur.}
			- **Character Development**: {Summarize the character development occurring in this chapter}
			- **Key Events**: {List and briefly describe the key events of the chapter}
			- **Chapter Conclusion**: {Sum up the chapter's impact on the overall narrative and set up the next chapter}
			""";
	}
}
