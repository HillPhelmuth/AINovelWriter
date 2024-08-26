using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AINovelWriter.Shared.Models
{
	public class Prompts
	{
		public class Authors
		{
			public const string JoeAbercrombie =
				"""
				Joe Abercrombie is a British fantasy author who has gained a strong following for his gritty, realistic approach to the genre. His writing style, often described as "grimdark," subverts many traditional fantasy tropes.
				**Key aspects of Abercrombie's action writing:**
				
				1. Realism: His fight scenes are brutal, messy, and often unglamorous, reflecting the true nature of violence.
				2. Character focus: Combat in his books isn't just about the physical action, but also about the psychological impact on the characters.
				3. Moral ambiguity: There are rarely clear-cut heroes or villains in his fights, reflecting his overall theme of moral complexity.
				4. Tactical detail: Abercrombie often includes intricate details about fighting techniques and battlefield tactics.
				5. Consequences: His stories don't shy away from showing the aftermath of violence, including injuries, trauma, and long-term effects.
				
				Notable works include "The First Law" trilogy and the standalone novels set in the same world, such as "The Heroes," which is essentially one long battle scene.
				""";

			public const string LeeChild =
				"""
				Lee Child, born James Dover Grant,  best known for his Jack Reacher thriller series. 
				**Key aspects of Child's action writing:**
				
				1. Precise descriptions: Child often uses short, punchy sentences to describe action, creating a sense of immediacy and tension.
				2. Technical detail: He incorporates specific details about weapons, fighting techniques, and physical principles, adding authenticity to his scenes.
				3. Strategic thinking: Reacher often analyzes situations before engaging, giving readers insight into the tactical aspects of combat.
				4. Physical realism: While Reacher is exceptionally skilled, the fights are grounded in reality, with factors like size, reach, and environment playing crucial roles.
				5. Pacing: Child is skilled at building tension leading up to confrontations and varying the rhythm of his action scenes.
				
				The Jack Reacher series has been hugely successful, with many of the books adapted into films and a streaming series. Child's writing style, particularly his action scenes, has been praised for its engaging, cinematic quality.
				""";
			public const string StevenErikson =
				"""
			    Steven Erikson (Malazan Book of the Fallen)
			    **Key aspects of action writing style:**
			    1 Epic Scale and Scope: Erikson's battles are often sprawling, involving hundreds, thousands, even tens of thousands of combatants. He masterfully orchestrates these massive conflicts, shifting perspectives between individual soldiers, squad leaders, and overall commanders, giving the reader a sense of the vastness and chaos of war.
			    2. Magic as a Force of Nature: Magic in the Malazan world is a raw, powerful force, and its use in battle is often awe-inspiring and terrifying. Mages can summon storms, unleash devastating blasts of energy, and even manipulate the very fabric of reality. These magical elements add a layer of unpredictability and spectacle to Erikson's battles.
			    3. Focus on the Emotional Toll: Erikson doesn't shy away from depicting the horrors of war and the profound impact it has on his characters. His battle scenes are not just about physical clashes; they are also about the psychological and emotional struggles of the individuals involved.
			    4. Intricate and Layered: Erikson's battles are often complex and multi-layered, with shifting alliances, hidden agendas, and unexpected twists. He keeps the reader guessing, constantly raising the stakes and challenging expectations.                                
			    """;

			public const string BrandonSanderson =
				"""
				Brandon Sanderson (Mistborn, Stormlight Archive)
				**Key aspects of action writing style:**
				1. Clarity and Precision: Sanderson's fight scenes are known for their clarity and precision. He meticulously describes the movements and techniques of his characters, making the action easy to follow and visualize.
				2. Innovative Magic Systems: Sanderson is renowned for his intricate and inventive magic systems, which play a significant role in his combat scenes. In Mistborn, characters use metal-based powers to enhance their physical abilities and manipulate the environment. In Stormlight Archive, characters bond with powerful spirits and wield incredible abilities fueled by stormlight.
				3. Emphasis on Strategy and Tactics: Sanderson's battles are not just about brute force; they often involve clever strategies and tactical maneuvers. His characters use their unique abilities and the environment to their advantage, creating dynamic and engaging confrontations.
				4. Focus on Character Growth: Sanderson uses action scenes as opportunities for character development. His characters often face overwhelming odds and must overcome their limitations to succeed. These trials forge them into stronger, more capable individuals.                          
				""";
		}
		public const string ChapterOutlineEditorPrompt =
			"""
			# Objective
			You are an assistant for a novel author. You have been tasked with expanding the outline for the next chapter of a novel. The author has provided you the details for the next chapter and overall story elements. Your job is to expand the details into a more detailed chapter outline with 3 distinct sections. Each section should be in a similar format of the chapter outline and contain **setting**, **character development**, **key events**.
			Your response should always begin with the chapter name using markdown header 2 (e.g. ## Chapter 1 - The Beginning), and then use a markdown line (`---`) to distinguish each sections.
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
			$$$"""
            ## Instructions
            
            - Rewrite the chapter below by applying the Feedback suggestions. 
            - The chapter must follow the writing style guide provided.
            - The Feedback includes Strengths, Weaknesses, and Suggestions. Re-write the chapter to apply the Suggestions to improve on the Weaknesses while retaining the Strengths.
            - Do not simply regurgitate the chapter. Rewrite it to improve the overall quality.
            
            ## Writing Style Guide
            
            {{{StyleGuide}}}
          
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
            $$$"""
			## Instructions
			         
			1. Write only 1 long and very detailed chapter based on the Chapter Outline.
			        
			2. You will be provided with the exact text of the Previous Chapter, if available. You must maintain consistency and continuity with the previous chapter content and tone.
			        
			3. You will be provided with a summary of the novel's events from before the previous chapter. You must maintain consistency with the novel's previous events and developments.
			        
			4. Use the Story Description and Characters to inform your writing.

			5. Unless otherwise indicated in the Story Description, you should write at a level suitable for a general audience. Generally aim for a PG-13 rating.

			6. Unless otherwise indicated, avoid explicit language, graphic violence, and sexual content. However, if the Story Description or Outline includes such elements, you should include them in your writing. You are also enouraged to use them if the Story Description clearly indicates a dark or adult theme.
			
			7. The chapter should be 10 to 15 pages.

			8. The chaper should start with the chapter name and number in the format: `## Chapter 1: {first chapter name}.`
			
			## Writing Style Guide
			
			{{{StyleGuide}}}
			        
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
			- Follow the writing style guide provided.
			- Keep in mind the Elements of a Good Chapter.
			
			Now, take a deep breath and start writing with an authentic voice. 
			""";
		public const string ChapterWriterPrompt2 =
			$$$"""
			## Instructions
			         
			1. Write only 1 long and very detailed chapter based on the Chapter Outline.
			        
			2. You will be provided with the exact text of the Previous Chapter, if available. You must maintain consistency and continuity with the previous chapter content and tone.
			        
			3. You will be provided with a summary of the novel's events from before the previous chapter. You must maintain consistency with the novel's previous events and developments.
			        
			4. Use the Story Description and Characters to inform your writing.
			
			5. Unless otherwise indicated in the Story Description, you should write at a level suitable for a general audience. Generally aim for a PG-13 rating.
			
			6. Unless otherwise indicated, avoid explicit language, graphic violence, and sexual content. However, if the Story Description or Outline includes such elements, you should include them in your writing. You are also enouraged to use them if the Story Description clearly indicates a dark or adult theme.
			
			7. The chapter should be 10 to 15 pages.
			
			8. The chaper should start with the chapter name and number in the format: `## Chapter 1: {first chapter name}.`
			
			## Writing Style Guide
			
			{{{StyleGuide}}}
			
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
			- Follow the writing style guide provided.
			- Keep in mind the Elements of a Good Chapter.
			
			Now, take a deep breath and start writing with the provided voice. 
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
			- Include sub-sections for Setting, Character Development, and Key Events.
			
			# Outline of a Chapter
					
			## Chapter 1: {Name of Chapter}
					
			1. **POV Character**: {Name of the character whose point of view the chapter is written from, if applicable}
			2. **Setting and Atmosphere:**: Introduce the setting where the primary events occur.
			 - _Vivid Description:_  Use sensory details to bring the setting to life and create a specific mood or atmosphere.
			 - _Relevance:_ The setting should be more than just backdrop; it should influence the characters and the events of the chapter.
			3. **Character Development**: Summarize the character development occurring in this chapter
			 - _Growth and Change:_  Chapters should reveal something new about the characters, their motivations, or their relationships. They should evolve, even if minimally.
			 - _Authenticity:_ Characters should behave and react in believable ways, given their personalities and circumstances.
			 - _Point of View:_ Maintaining a consistent point of view helps readers connect with the characters and their experiences.
			4. **Plot Development:** Provide and describe in detail several plot developments of the chapter
			 - _Movement:_ Something needs to happen. This could be an external event, an internal conflict, a revelation, or a shift in relationships.
			 - _Purpose:_ The chapter should contribute to the overall plot of the novel. It should move the story forward, even if subtly.
			 - _Conflict and Tension:_ Introduce or escalate conflict, whether internal or external. This creates tension and keeps the reader invested.
			
			""";
		public const string OutlineReversePrompt =
			"""
			Write an outline of the chapter using the format below.
			
			# Outline of a Chapter
					
			## Chapter 1: {Name of Chapter}
					
			1. **POV Character**: {Name of the character whose point of view the chapter is written from, if applicable}
			2. **Setting and Atmosphere:**: Introduce the setting where the primary events occur.
			 - _Vivid Description:_  Use sensory details to bring the setting to life and create a specific mood or atmosphere.
			 - _Relevance:_ The setting should be more than just backdrop; it should influence the characters and the events of the chapter.
			3. **Character Development**: Summarize the character development occurring in this chapter
			 - _Growth and Change:_  Chapters should reveal something new about the characters, their motivations, or their relationships. They should evolve, even if minimally.
			 - _Authenticity:_ Characters should behave and react in believable ways, given their personalities and circumstances.
			 - _Point of View:_ Maintaining a consistent point of view helps readers connect with the characters and their experiences.
			4. **Plot Development:** Provide and describe in detail several plot developments of the chapter
			 - _Movement:_ Something needs to happen. This could be an external event, an internal conflict, a revelation, or a shift in relationships.
			 - _Purpose:_ The chapter should contribute to the overall plot of the novel. It should move the story forward, even if subtly.
			 - _Conflict and Tension:_ Introduce or escalate conflict, whether internal or external. This creates tension and keeps the reader invested.
						
			 # Chapter Text
			 ```
			 {{$novel_chapter}}
			 ```
			""";
        public const string SummaryPrompt =
            """
            ## Instructions
            Summarize all the character details and plot events in the novel chapter. Use the following template for your response.
            
            ## Output Template

            **Character Developement:** {Describe the character developments in the chapter}
            **Character Interactions:** {Describe the character interactions in the chapter}
            **Plot Events:** {Describe all the plot events in the chapter}
            
            ## Chapter Text
            {{$novel_chapter}} 
            ```
            """;

        public const string StyleGuide =
	        """
	        1. **Detailed Descriptions and Proper Pace:** You will prioritize **very** detailed descriptions of the setting, characters, and events. Imagine the reader is experiencing the story through their senses – sight, sound, touch, smell, and taste. Paint a vivid picture with your words. You will also prefer a slower pace of events, focusing on character development, inner thoughts, and nuanced interactions. Allow the reader to fully immerse themselves in the world and connect with the characters on a deeper level.
	        
	        2. **Cinematic Action: Immerse Your Reader in the Fight:**
	        
	        Don't just tell us about a fight – unleash it on the page!  Craft exhilarating action sequences that crackle with energy and keep readers on the edge of their seats.
	        
	        **Instead of:** "They fought fiercely."
	        
	        **Unleash the action:**  "Steel clashed against steel, a shower of sparks erupting in the dim light.  He parried a lightning-fast thrust, the force of the blow vibrating up his arm.  Adrenaline surged through him, sharpening his senses.  He ducked, weaved, and countered, each movement a calculated dance of death.  He could feel the burn in his muscles, the sting of sweat in his eyes.  His opponent, a whirlwind of aggression, pressed the attack, forcing him to retreat.  He needed an opening, a chance to strike… now!"
	        
	        **Remember to:**
	        
	        * **Choreograph the Combat:** Describe each movement with precision, focusing on the impact of blows, the clash of weapons, and the grunts of exertion.
	        * **Engage the Senses:**  Immerse the reader in the sights, sounds, smells, and even the feel of the fight.  Think dust kicked up by frantic footwork, the metallic tang of blood, the roar of the crowd (if applicable).
	        * **Strategic Depth:**  Give insights into the characters' tactical thinking, their split-second decisions, and the reasons behind their actions.
	        * **Pacing and Tension:** Build suspense, allow moments of respite, and then escalate the conflict towards a thrilling climax.
	        
	        By crafting cinematic action sequences, you'll transform your readers into active participants, drawing them into the heart of the battle.
	        
	        3.  **Give Your Characters a Voice: Dialogue That Sings**
	        
	        Let your characters speak for themselves! Craft distinct and believable voices that breathe life into their personalities and propel the narrative forward.
	                
	         **Don't just have them talk:** "He said he was happy to be there."
	                
	         **Let their voices resonate:**
	                
	          * **The gruff veteran:** "Happy? Kid, I've seen happier faces at a funeral.  Let's just get this over with."
	          * **The eager apprentice:** "Oh, wow! This is amazing! I can't wait to learn everything!"
	          * **The sly rogue:** "Happy?  Let's just say I'm... *delighted* to be of service.  For a price, of course."
	                
	          **Remember to:**
	                
	          * **Reflect Personality:**  How does their word choice, tone, and slang reveal who they are?
	          * **Consider Background:**  Does their upbringing, education, or social status influence how they speak?
	          * **Driven by Motivation:**  What are their goals and desires?  How does this affect their dialogue?
	          * **Relationship Dynamics:**  How does their speech change when interacting with different characters?
	          * **Advance the Plot:**  Use dialogue to reveal information, create conflict, and move the story forward.
	          * **Emotional Impact:**  Can dialogue be used to inject humor, build tension, or evoke empathy?
	                
	        By crafting compelling dialogue, you'll not only bring your characters to life but also enrich the narrative tapestry of your story.
	        
	        4. **Bring Your Story to Life: Show, Don't Tell:**
	        
	         Instead of simply stating emotions or facts, immerse your reader in the scene. Use vivid descriptions that engage their senses. 
	        
	         **Don't tell us:** "He was angry."
	        
	         **Show us:** "His knuckles whitened as his fists clenched, a vein throbbing in his temple. A flush crept up his neck, painting his face a mottled crimson.  His voice, a strained rasp, vibrated with barely suppressed fury."
	        
	         **Remember to consider:**
	        
	         * **Sensory Details:** What can the characters see, hear, smell, taste, and touch?
	         * **Actions:**  What are the characters doing that reveals their emotions and intentions?
	         * **Body Language:** How are their postures, facial expressions, and gestures communicating?
	         * **Dialogue:** Can you reveal information through what the characters say (and don't say) to each other?
	        
	         By painting a picture with words, you'll create a more compelling and engaging experience for your reader.
	        
	        5.  Weave a Seamless Tapestry: Consistency and Continuity
	        
	        Imagine a beautiful tapestry, carefully woven with intricate details and vibrant threads.  Every element contributes to the overall picture, creating a harmonious and captivating whole.  Your story is no different.
	        
	        **Consistency and continuity are the threads that hold your narrative together, ensuring a believable and immersive experience for your reader.**
	        
	        **Consider these key elements:**
	        
	         * **World-Building:**  Establish clear rules for your world, from magic systems to social hierarchies, and maintain them consistently.
	         * **Character Traits:**  Ensure your characters behave in a way that aligns with their established personalities and motivations.  Avoid sudden, unexplained shifts in behavior.
	         * **Plot Details:**  Keep track of key events, timelines, and previously established information.  Avoid contradictions and inconsistencies that can break the reader's immersion.
	         * **Cause and Effect:**  Ensure actions have consequences and that events flow logically from one another.  This creates a sense of realism and reinforces the reader's engagement.
	        
	        **By weaving a tapestry of consistency and continuity, you'll create a world that feels real, characters that resonate, and a story that captivates from beginning to end.**
	        
	        By following these guidelines, you will create a richly detailed and immersive story that captivates the reader from beginning to end. Remember to prioritize both detailed descriptions and exciting action sequences, creating a balanced and engaging narrative.
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
