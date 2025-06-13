# AI Ghost-Writer Pro (The Milli Vanilli Project)

## 🚀 An AI Novel Writing Experiment

**Transform your story ideas into complete novels with the power of AI!**

## [🌐 Try it out Live!](https://ainovelwriter.azurewebsites.net/)



## 📖 What is AI Ghost-Writer Pro?

AI Ghost-Writer Pro is an innovative web application that leverages multiple AI models to generate complete novels from simple story concepts. Inspired by the idea of "ghost writers" this tool handles the heavy lifting of novel creation while you (sometimes) provide the creative direction. Just like Milli Vanilli lip-synced to someone else's vocals, you can now claim authorship of AI-generated literary masterpieces! 😉

(_Note: I do not encorage claiming authorship of AI-generated creative content - no matter how cool the app you used to generate it might be...<br>Though, now I suppose I have to admit that 75% of this README file is AI generated_)

## ✨ Key Features

- **Multi-AI Model Support**: Integrates with OpenAI GPT models, Google Gemini, MistralAI, and Amazon Bedrock
- **Intelligent Story Development**: Progressively builds stories from basic concepts to full novels
- **Interactive Web Interface**: Clean, modern UI built with Blazor Server and Radzen components
- **Real-time Generation**: Watch your novel being written in real-time with streaming text
- **Audio Integration**: Text-to-speech functionality to listen to your generated novels
- **Cover Art Generation**: AI-generated cover art for your stories
- **Export Options**: Download completed novels in various formats

## 🏗️ Application Architecture

The application is built using:
- **Frontend**: Blazor Server with Radzen UI components
- **Backend**: ASP.NET Core with Semantic Kernel
- **AI Integration**: Multiple AI providers (OpenAI, Google, xAI, MistralAI)
- **Authentication**: Auth0 integration
- **Audio Processing**: Text-to-speech capabilities
- **Cloud Storage**: Azure Blob Storage for assets
- **Cosmos Db**: Store saved and shared novels

## 📝 The Novel Generation Process

The AI Novel Writer follows a systematic 6-step process to transform your ideas into complete novels:

### 1. **Story Concept Creation** 📋
![Concept Phase](Images/ConceptsPage.png)

**How it works:**
- Users input basic story parameters: genre, tone, audience, length
- Select from various subgenres and personality traits
- The system uses the [`GenerateNovelIdea()`](AINovelWriter.Shared/Services/NovelWriterService.cs#L116-159) method to create initial concepts
- AI generates a structured [`NovelConcepts`](AINovelWriter.Shared/Models/NovelConcepts.cs) object containing foundational story elements

**User Input Options:**
Users can either manually input the foundational story elements or have AI generate them individually:
- Title generation ([`GenerateNovelTitle()`](AINovelWriter.Shared/Services/NovelWriterService.cs#L161-171)) - Create compelling book titles
- Theme/description development ([`GenerateNovelDescription()`](AINovelWriter.Shared/Services/NovelWriterService.cs#L173-182)) - Develop core story themes
- Character creation ([`GenerateNovelCharacters()`](AINovelWriter.Shared/Services/NovelWriterService.cs#L184-193)) - Generate detailed character profiles
- Plot event planning ([`GenerateNovelPlotEvents()`](AINovelWriter.Shared/Services/NovelWriterService.cs#L195-204)) - Create key story events and plot points

### 2. **Outline Development** 📊
![Outline Phase](Images/OutlinePage.png)

**How it works:**
- The AI expands the complete concept (whether user-input or AI-generated) into a detailed chapter-by-chapter outline
- Uses the `CreateNovelOutline()` method with specialized [prompts](AINovelWriter.Shared/Models/Prompts.cs#L436)
- Incorporates user-provided additional instructions
- Generates structured plot progression and character development arcs

### 3. **Chapter-by-Chapter Writing** ✍️
![Writing Phase](Images/NovelStream.png)

**How it works:**
- The [`WriteFullNovel()`](AINovelWriter.Shared/Services/NovelWriterService.cs#L241) method streams the novel generation in real-time
- Each chapter is written using the [`WriteChapterStreaming()`](AINovelWriter.Shared/Services/NovelWriterService.cs#L338) method
- Maintains story continuity by passing previous chapter summaries
- Uses advanced prompting with style guides for consistent quality

### 4. **Audio Integration** 🎧

**How it works:**
- Completed chapters are converted to audio using text-to-speech
- Multiple voice options and playback controls
- Chapter-by-chapter audio navigation
- Implemented through the `AudioService` component

### 5. **Cover Image Generation**
![Cover Image](Images/CoverGen.png)

**Hot it works:**
- Select one or two art style options.
- Add optional custom instructions
- Uses novel details along with selected styles and instructions in prompt for `gpt-image-1` 

### 6. **Final Output & Export** 📚
![Final Phase](Images/BookReader.png)

**How it works:**
- Complete novel with professional formatting and page turning animation
- Export options for different file formats (pdf or epub)
- Generated cover art integration
- Novel evaluation and quality metrics

## 🛠️ Technical Implementation

### Core Services

**NovelWriterService**: The main orchestrator that handles the entire novel generation pipeline
- Manages AI model selection and switching
- Handles prompt engineering and response processing
- Implements streaming for real-time user feedback
- Coordinates between different generation phases

**AI Model Integration**: 
Each section has its default model that is best-suited to the task, but the user can opt to select a different model.
- **OpenAI**: Primary models for diff-based modifications and for all evaluations.
- **Google Gemini**: Fast outline and concept generation  
- **xAI Grok-3**: Primary model for novel writing
- **MistralAI**: Alternative creative writing model

**[Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/) Integration**:
- Plugin-based architecture for extensibility
- Advanced prompt templating and execution
- Function calling and tool integration
- Conversation history management

### Error Handling & Resilience

- JSON parsing repair for malformed AI responses
- Retry logic with Polly for API failures
- Model fallback strategies
- Graceful degradation for partial failures


## 🔮 Additional Features

### **AI Novel Review & Evaluation** 📊
The application includes a sophisticated review system that provides comprehensive analysis of generated novels:

**Review Types Available:**
- **Full Coverage: Comprehensive Analysis, In-Depth Review** - Complete literary analysis
- **Literary Criticism: Academic Purposes, Thematic Exploration** - Scholarly examination
- **Book Reviewing for Publications: Professional Book Critics, Consumer Reviews** - Publication-ready reviews
- **Market Research: Publishing Industry** - Commercial viability assessment
- **Literary Awards and Recognition: Award Committees** - Competition-level evaluation

**Review Features:**
- **Logline Analysis** - Concise story summaries and hook evaluation
- **Synopsis Generation** - Professional plot summaries
- **Chapter-by-Chapter Breakdown** - Detailed scene and character analysis
- **Strengths & Weaknesses Assessment** - Constructive feedback identification
- **Improvement Suggestions** - Actionable recommendations for enhancement

### **AI Assisted Revisions** ✏️
Advanced editing capabilities that allow users to refine their generated content:

**Revision Options:**
- **Chapter Selection** - Choose specific chapters for targeted improvements
- **Context-Specific Edits** - AI understands the broader story context
- **Multiple AI Models** - Different models optimized for different types of revisions
- **Additional Instructions** - Custom guidance for specific revision goals
- **Iterative Refinement** - Multiple revision passes for continuous improvement

### **AI Editor Agent** 🤖
An intelligent collaborative editor that acts as your writing partner:

**Interactive Features:**
- **Real-time Feedback** - Immediate suggestions and improvements
- **Creative Partnership** - Acts as a collaborative writing companion
- **Structural Analysis** - Identifies plot holes, pacing issues, and character development opportunities
- **Style Consistency** - Maintains voice and tone throughout the work
- **Professional Guidance** - Expert-level editorial advice from concept to completion

**Editor Capabilities:**
- Sharp eye for fiction and storytelling elements
- Identification of big-picture to fine-line level improvements
- Bold revision suggestions while respecting creative vision
- Developmental editing for character arcs and worldbuilding
- Line-level editing for dialogue, prose style, and flow


---

> *"Why write a novel when AI can do it for you? Just like Milli Vanilli, you get all the credit!"* 😄<br>
 -- Nobody, ever