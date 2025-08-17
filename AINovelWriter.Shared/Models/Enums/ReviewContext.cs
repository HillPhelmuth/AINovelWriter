using System.ComponentModel;

namespace AINovelWriter.Shared.Models.Enums;

public enum ReviewContext
{
    [Description("You Choose!")]
    None,
    [Description("Full Coverage: Comprehensive Analysis, In-Depth Review")]
    [Prompt(Prompts.NovelFullCoverageReviewPrompt)]
    FullCoverage,
    [Description("Literary Criticism: Academic Purposes, Thematic Exploration")]
    [Prompt("Analyze this novel with a focus on its literary elements, such as theme, narrative technique, and its place in the literary canon. Provide a thoughtful critique, drawing comparisons to other significant works if relevant.")]
    LiteraryCriticism,

    [Description("Book Reviewing for Publications: Professional Book Critics, Consumer Reviews")]
    [Prompt("Write a book review that would be suitable for publication in a newspaper or literary magazine. Evaluate the novel's strengths and weaknesses, and provide a recommendation for potential readers.")]
    BookReviewing,

    [Description("Market Research: Publishing Industry")]
    [Prompt("Assess this novel's potential marketability. Consider factors such as genre trends, target audience, and its commercial appeal in today's publishing landscape.")]
    MarketResearch,

    [Description("Literary Awards and Recognition: Award Committees")]
    [Prompt("Evaluate this novel as if you were on a literary award committee. Discuss its literary merit, originality, and overall impact. Would you consider it for a shortlist or an award?")]
    LiteraryAwards,

    [Description("Educational Purposes: Teachers and Educators, Curriculum Development")]
    [Prompt("Provide an analysis of this novel with a focus on its suitability for educational use. Discuss its themes, language, and relevance to a specific curriculum or course of study.")]
    EducationalPurposes,

    [Description("Personal Interest: Entertainment, Exploration of a Genre")]
    [Prompt("Review this novel from the perspective of a passionate reader. Discuss what makes it enjoyable or engaging, especially in terms of its genre and storytelling.")]
    PersonalInterest,

    [Description("Advocacy and Social Commentary: Promoting Social Issues, Representation Matters")]
    [Prompt("Analyze this novel with an emphasis on its social and political themes. Discuss how it addresses issues such as race, gender, and social justice, and its potential impact on readers.")]
    Advocacy,

    [Description("Cultural and Historical Context: Cultural Analysis, Historical Relevance")]
    [Prompt("Examine this novel within its cultural and historical context. Discuss how it reflects or challenges the norms and values of its time, and its relevance to modern readers.")]
    CulturalHistoricalContext,

    [Description("Promotional or Marketing Efforts: Author and Publisher Marketing, Influencers and Bloggers")]
    [Prompt("Write a review designed to generate excitement and interest in this novel. Focus on its unique features, appeal, and why it stands out in the current literary market.")]
    PromotionalMarketing
}