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

        public const string ChapterImprovementPrompt =
            """
            ## Instructions
            Provide feedback as a novel editor. Locate the flaws and provide notes for a re-write, if necessary.
            
            Feedback should have three sections: Strengths, Weaknesses, and Suggestions for Improvement. Each section should be detailed and presented in markdown format.
            
            The user may have notes as well. Consider these notes when providing feedback.
            
            ## Output
            Overall output should be in json format. Use the template below for your response.
            
            ```json
            {
                "Strengths": "Your strengths feedback in markdown format here.",
                "Weaknesses": "Your weaknesses feedbackin markdown format here.",
                "Suggestions": "Your suggestions feedbackin markdown format here."
            }
            ```
            
            ## User Notes
            
            {{ $notes }}
            
            ## Chapter Text
            
            {{ $chapterText }}
            
            """;

        public const string ChapterRewritePrompt =
            """
            ## Instructions
            
            - Rewrite the chapter below by applying the Feedback suggestions. 
            - The chapter must follow the writing style guide provided.
            - The Feedback includes Strengths, Weaknesses, and Suggestions. Re-write the chapter to apply the Suggestions to improve on the Weaknesses while retaining the Strengths.
            - Do not simply regurgitate the chapter. Rewrite it to improve the overall quality.
            
            ## Writing Style Guide
            
            1. **Detailed Descriptions and Slower Pace:** You will prioritize **very** detailed descriptions of the setting, characters, and events. Imagine the reader is experiencing the story through their senses – sight, sound, touch, smell, and taste. Paint a vivid picture with your words. You will also prefer a slower pace of events, focusing on character development, inner thoughts, and nuanced interactions. Allow the reader to fully immerse themselves in the world and connect with the characters on a deeper level.
            
            2. **Extended and Exciting Action Sequences:** When action occurs, it should be **very** detailed and exciting. Describe the movements - every movement, the impact of blows - blow by blow, the adrenaline rush, and the strategic choices - and reasoning behind them - made during the fight. Don't shy away from using vivid imagery and sensory details to make the reader feel like they are in the midst of the action.  These action sequences should be extended and well-paced, building tension and suspense before reaching a thrilling climax.
            
            3. **Character Voice and Dialogue:** Develop distinct and believable voices for each character. Their dialogue should reflect their personality, background, and motivations. Use dialogue to reveal character relationships, advance the plot, and provide moments of humor or tension.
            
            4. **Show, Don't Tell:**  Instead of simply stating facts, use descriptive language to show the reader what is happening. For example, instead of saying "He was angry," describe his clenched fists, reddened face, and strained voice.
            
            5. **Consistency and Continuity:** Maintain consistency in your world-building, character traits, and plot details. Pay attention to continuity to ensure a seamless and believable reading experience.
          
            ## Feedback
            Rewrite the chapter using the following feedback. 
            
            ### Strengths
            {{ $strengths }}
            
            ### Weaknesses
            {{ $weaknesses }}
            
            ### Suggestions
            {{ $suggestions }}
            
            ## Chapter to Rewrite
            
            {{ $chapterText }}
                                        
            """;
		public const string ChapterWriterPrompt =
			"""
			You are a creative and exciting fiction writer. You write intelligent, detailed and engrossing novels that expertly combine character development and growth, interpersonal and international intrigue, and thrilling action.
			        
			## Instructions
			         
			1. Write only 1 long, detailed chapter based on the Chapter Outline.
			        
			2. You will be provided with the exact text of the Previous Chapter, if available. You must maintain consistency and continuity with the previous chapter content and tone.
			        
			3. You will be provided with a summary of the novel's events from before the previous chapter. You must maintain consistency with the novel's previous events and developments.
			        
			4. Use the Story Description and Characters to inform your writing.

			5. Unless otherwise indicated in the Story Description, you should write at a level suitable for a general audience. Generally aim for a PG-13 rating.

			6. Unless otherwise indicated, avoid explicit language, graphic violence, and sexual content. However, if the Story Description or Outline includes such elements, you should include them in your writing. You are also enouraged to use them if the Story Description clearly indicates a dark or adult theme.
			
			7. The chapter must be 10 to 20 pages.

			8. The chaper should start with the chapter name and number in the format: `## Chapter 1: {first chapter name}.`
			
			## Writing Style Guide
			
			1. **Detailed Descriptions and Slower Pace:** You will prioritize **very** detailed descriptions of the setting, characters, and events. Imagine the reader is experiencing the story through their senses – sight, sound, touch, smell, and taste. Paint a vivid picture with your words. You will also prefer a slower pace of events, focusing on character development, inner thoughts, and nuanced interactions. Allow the reader to fully immerse themselves in the world and connect with the characters on a deeper level.
			
			2. **Extended and Exciting Action Sequences:** When action occurs, it should be **very** detailed and exciting. Describe the movements - every movement, the impact of blows - blow by blow, the adrenaline rush, and the strategic choices - and reasoning behind them - made during the fight. Don't shy away from using vivid imagery and sensory details to make the reader feel like they are in the midst of the action.  These action sequences should be extended and well-paced, building tension and suspense before reaching a thrilling climax.
			
			3. **Character Voice and Dialogue:** Develop distinct and believable voices for each character. Their dialogue should reflect their personality, background, and motivations. Use dialogue to reveal character relationships, advance the plot, and provide moments of humor or tension.
			
			4. **Show, Don't Tell:**  Instead of simply stating facts, use descriptive language to show the reader what is happening. For example, instead of saying "He was angry," describe his clenched fists, reddened face, and strained voice.
			
			5. **Consistency and Continuity:** Maintain consistency in your world-building, character traits, and plot details. Pay attention to continuity to ensure a seamless and believable reading experience.
			
			By following these guidelines, you will create a richly detailed and immersive story that captivates the reader from beginning to end. Remember to prioritize both detailed descriptions and exciting action sequences, creating a balanced and engaging narrative.
			        
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
			- Do not number the paragraphs. It should flow just like a novel.

			Now, take a deep breath and start writing step by step.
			""";
		public const string ChapterWriterPrompt2 =
			"""
			You are a creative and exciting fiction writer. You write intelligent and engrossing novels that expertly combine character development and growth, interpersonal and international intrigue, and thrilling action.
			        
			## Instructions
			         
			1. Write only 1 long chapter based on the Chapter Outline.
			        
			2. You will be provided with the exact text of the novel up to this point, if available. You must maintain consistency and continuity with the previous chapters' content and tone.
			        
			3. Use the Story Description and Characters to inform your writing.

			4. Unless otherwise indicated in the Story Description, you should write at a level suitable for a general audience.

			5. Unless otherwise indicated, avoid explicit language, graphic violence, and sexual content. However, if the Story Description or Outline includes such elements, you should include them in your writing. You are also enouraged to use them if the Story Description clearly indicates an adult theme.
			        
			6. The chapter must contain about 100 paragraphs.

			7. The chaper should start with the chapter name and number in the format: `## Chapter 1: {first chapter name}.`
			        
			## Story Description and Characters
			        
			{{ $storyDescription }}
			        
			## Novel so far
			        
			{{ $summary }}
			        
			## Chapter Outline

			```
			{{ $outline }}
			```

			## Objectives

			- Write a compelling long chapter for the novel based on the critera above.
			- It must be 100 paragraphs long.
			- Do not number the paragraphs. It should flow just like a novel.

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
			- Ensure each chapter's outline does not exceed 600 words.
					
			# Outline of a Chapter
					
			## Chapter 1: {Name of Chapter}
					
			- **Setting**: {Introduce the setting where the primary events occur.}
			- **Character Development**: {Summarize the character development occurring in this chapter}
			- **Key Events**: {List and briefly describe at least 5 key events of the chapter}
			- **Chapter Conclusion**: {A clear endpoint for the chapter designed to set up the next chapter}
			""";
        public const string OutlineWriterPromptJson =
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
            - Use json format for the outline.
            - Each chapter should have a string property for the chapter name.
            - Each chapter should have a string property for the setting - This will introduce the setting where the primary events occur.
            - Each chapter should have a string property for the character development - This will summarize the character development occurring in this chapter.
            - Each chapter should have a string array property for the key events - This will list and briefly describe at least 5 key events of the chapter.
            - Each chapter should have a string property for the chapter conclusion - This will provide a clear endpoint for the chapter designed to set up the next chapter.
            
            """;

        public const string OutlineSchema =
            """
            {
            	"type": "json_schema",
            	"json_schema": {
            		"name": "write_chapters",
            		"schema": {
            			"type": "object",
            			"properties": {
            				"chapters": {
            					"type": "array",
            					"items": {
            						"type": "object",
            						"properties": {
            							"chapterName": {
            								"type": "string"
            							},
            							"setting": {
            								"type": "string"
            							},
            							"characterDevelopment": {
            								"type": "string"
            							},
            							"keyEvents": {
            								"type": "array",
            								"items": {
            									"type": "string"
            								}								
            							},
            							"chapterConclusion": {
            								"type": "string"
            							}
            						},
            						"required": [
            							"chapterName",
            							"setting",
            							"characterDevelopment",
            							"keyEvents",
            							"chapterConclusion"
            						]
            					}
            				}
            			},
            			"required": [
            				"chapters"
            			]
            		},
            		"strict": true
            	}
            }
            """;
    }
}
