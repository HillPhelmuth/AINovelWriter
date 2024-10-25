using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AINovelWriter.Shared.Models
{
    public class Prompts
    {
        public class AgentPrompts
        {
            public const string EditorAgentPrompt =
            """
            ## Your Role: Collaborative Novel Editor
            
            Your role is to act as a collaborative editor for a novel, working closely with the user to elevate their storytelling and refine their manuscript. You will provide insightful feedback, suggest creative improvements, and implement revisions based on your expertise, the user's preferences, and the available tools. You are capable of analyzing narrative structure, character development, pacing, dialogue, style, and all other elements that contribute to a compelling and well-written novel.  
            
            **Your primary objective is to guide the user through an iterative editing process, ensuring their vision is realized while enhancing the overall quality and impact of their work.**
            
            **Getting Started:**
            
            Begin by understanding the user's current stage in the writing process and their goals for the novel. 
            
            * Ask the user about their manuscript: 
                * "Could you tell me about your novel? What's the core story about?"
                * "What stage is your manuscript currently in? (e.g., first draft, revisions, etc.)"
                * "What are your primary goals for this editing process?" 
            * Offer assistance with developing or refining the novel's structure:
                * "Do you have a complete outline or summary? I can help you create or refine one if needed using `GetFullNovelSummary` or by analyzing existing chapter content with `GetChapterText`."
            
            **Throughout the Editing Process:**
            
            1. **Gather Information:** Analyze the provided text (chapters, sections, or the entire manuscript). Utilize your understanding of storytelling principles, best practices, and the available tools (e.g., `GetNovelEvals`, `ReviewChapter`) to identify strengths and areas for improvement.
            2. **Provide Feedback:** Offer constructive and detailed feedback to the user, focusing on both macro-level elements (plot, structure, pacing) and micro-level elements (language, style, dialogue). 
            3. **Suggest Revisions:** Propose specific changes and improvements to the text, always respecting the user's creative vision. Explain the rationale behind your suggestions clearly. You can use tools like `CreateNewChapterOutlineFromFeedback` to aid in restructuring chapters if needed.
            4. **Implement Changes:** Upon receiving user approval, implement the agreed-upon revisions to the manuscript, potentially utilizing `GetChapterText` to retrieve and modify specific sections.
            5. **Collaborate Iteratively:** Continuously engage in a dialogue with the user. Seek clarification, answer questions, and adapt your approach based on their feedback and evolving needs.
            
            **Key Considerations:**
            
            * **User's Vision:** Prioritize the user's creative intentions and ensure all edits align with their overall vision for the novel.
            * **Constructive Feedback:**  Provide feedback that is both encouraging and insightful, empowering the user to improve their writing.
            * **Flexibility:** Adapt your approach based on the user's writing style, preferences, and the specific needs of their manuscript.
            * **Effective Tool Utilization:**  Leverage the available tools strategically to enhance your editing capabilities and streamline the process.
            * **Clarity and Communication:** Communicate your thoughts and suggestions clearly and effectively, ensuring a smooth and productive collaboration.
            
            **Personality Traits to Embody:**
            
            1. **Knowledgeable:** Demonstrate expertise in storytelling, editing, and writing techniques.
            2. **Arrogant:** Be extremely confident in your suggestions and feedback, but avoid being overly dismissive of the user's ideas.
            
            **Remember**: You are not just an editor; you are a creative partner, helping the user to unlock the full potential of their novel.
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
            
            Feedback should have three sections: Strengths, Weaknesses, and Suggestions for Improvement. Each section should be detailed and presented in markdown format. The Suggestions section should be actionable and specific, focusing on how the chapter can be improved with some specific examples.
            
            The user may have notes as well. Consider these notes when providing feedback.
            
            The feedback should consider the chapter in the context of the overall story. Use the Story Summary to understand the chapter's place in the narrative.
            
            ## Output
            Overall output should be in json format. Use the template below for your response.
            
            ```json
            {
                "Strengths": "Your strengths feedback in markdown format here.",
                "Weaknesses": "Your weaknesses feedback in markdown format here.",
                "Suggestions": "Your suggestions feedback in markdown format here."
            }
            ```
            **Important Note:** If User Notes and Chapter Text are empty strings, the feedback should simply state that the text is missing for each json property.
            
            ## User Notes
            
            {{ $notes }}
            
            ## Story Summary
            
            {{ $storySummary }}
            
            ## Chapter Text
            
            {{ $chapterText }}
            
            """;

        public const string NovelFullCoverageReviewPrompt =
            """
	        Provide comprehensive novel coverage for the following novel. I would like the coverage to be as detailed and insightful as possible, utilizing your advanced capabilities. Please adhere to the following structure:
	        
	        ## Output structure
	        
	        1. **Logline:** Craft a concise and compelling logline that captures the essence of the story.
	        
	        2. **Synopsis:** Provide a detailed synopsis of the plot, highlighting key turning points and character arcs.
	        
	        3. **Strengths:** Identify and elaborate on the novel's most impressive aspects. This may include elements such as:
	        
	        - **Concept:** Originality, marketability, and overall appeal of the story idea
	        - **Characters:** Depth, development, believability, and relatability of the characters
	        - **Structure:** Pacing, plot construction, act breaks, and overall narrative flow
	        - **Dialogue:** Naturalness, wit, subtext, and contribution to character development
	        - **Theme:** Clarity, relevance, and impact of the underlying message(s)
	        - **Emotional Impact:** Ability to evoke emotions, create tension, and resonate with the audience
	        
	        4. **Weaknesses:** Pinpoint areas where the novel could be improved, offering constructive solutions. Potential areas of focus could be:
	        
	        - **Plot Holes:** Inconsistencies, illogical developments, or unresolved storylines
	        - **Character Motivations:** Unclear or unconvincing reasons behind character actions
	        - **Pacing Issues:** Scenes or sequences that drag or feel rushed
	        - **Dialogue Problems:** Stilted, unnatural, or on-the-nose conversations
	        - **Lack of Clarity:** Confusing plot points, underdeveloped themes, or ambiguous character arcs
	        
	        5. **Overall Assessment & Recommendation:** Summarize your evaluation of the novel's potential. Provide a clear recommendation on whether the novel is considered:
	        
	        - **Pass:** The novel has significant flaws and requires substantial revisions.
	        - **Consider:** The novel shows promise but needs further development and refinement.
	        - **Recommend:** The novel is strong and ready to be moved forward in the development process.
	        - **Additional Considerations:**
	        
	        Please provide specific examples from the novel to support your analysis in each section.
	        Feel free to comment on any other aspects of the novel that you deem relevant, such as genre conventions or target audience.
	        
	        ## Novel Text
	        ```
	        {{ $novelText }}
	        ```
	        """;

        public const string NovelContextSpecificReviewPrompt =
            """
            ## Instructions
            Write a review of the novel below. Use the context instructions to guide your review. Always start with a concise and compelling **logline** that captures the essence of the story and a detailed **synopsis** of the plot, highlighting key turning points and character arcs.
            
            ## Context Instructions
            
            {{ $context }}
            
            ## Novel Text
            ```
            {{ $novelText }}
            ```
            """;
        public const string ChapterRewritePrompt =
            $$$"""
            ## Instructions
            
            - Rewrite the chapter below by applying the Feedback suggestions. 
            - The chapter must follow the writing style guide provided.
            - The Feedback includes Strengths, Weaknesses, and Suggestions. Re-write the chapter to apply the Suggestions to improve on the Weaknesses while retaining the Strengths.
            - The User Notes may provide additional context or instructions for the rewrite. If the user disagrees with the feedback, prioritize their perspective while making revisions.
            - Do not simply regurgitate the chapter. Rewrite it to improve the overall quality.
            
            ## Writing Style Guide
            
            {{{StyleGuide}}}
          
            ## Original Chapter Outline
            
            This is provided for context, but it is not required to follow the outline exactly. Use it as a reference for the chapter's structure and intended content.
            
            ```markdown
            {{ $chapterOutline }}
            ```
            
            ## Feedback
            Rewrite the chapter using the following feedback. 
            
            ### Strengths
            {{ $strengths }}
            
            ### Weaknesses
            {{ $weaknesses }}
            
            ### Suggestions
            {{ $suggestions }}
            
            ## User Notes
            
            {{ $notes }}
            
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
			
			7. Each chapter should contain significant advancements in both the main plot and any relevant subplots. Explore the complexities of character relationships, introduce new conflicts or challenges, and reveal crucial information that moves the story forward.
			
			8. Dedicate sufficient space within each chapter to explore the inner lives of your characters. Delve into their thoughts, motivations, and emotional responses to the unfolding events. Show how their experiences shape their growth and development throughout the story.
			
			9. Craft engaging and meaningful conversations between characters. Use dialogue to reveal their personalities, advance the plot, and explore the dynamics of their relationships. Ensure that each conversation serves a purpose and contributes to the overall narrative.
			
			10. Paint a vivid picture of the environment and atmosphere in each scene. Use descriptive language to immerse the reader in the world you've created. Explore the unique aspects of your setting and how they impact the characters and the story.

			11. The chaper should start with the chapter name and number in the format: `## Chapter 1: {first chapter name}.`
			
			
			        
			## Story Description, Characters and Key Events
			        
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

			- Write a compelling **long** chapter (2000 - 3000 words or 80 - 100 paragraphs) for the novel based on the critera above.
			- Follow the writing style guide provided.
			- Keep in mind the Writing Style Guide.
			
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
			Write an outline of the chapter using the format below. Only include accurate information from the Chapter Text
			
			# Format
					
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
						
			 # Chapter Text to use
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
            ```
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
