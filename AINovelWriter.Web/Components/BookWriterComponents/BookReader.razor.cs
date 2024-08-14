using Markdig;
using Microsoft.AspNetCore.Components;

namespace AINovelWriter.Web.Components.BookWriterComponents;

public partial class BookReader
{
    private readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    [Parameter]
    public string Text { get; set; } = "";
    
}