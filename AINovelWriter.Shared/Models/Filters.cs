using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace AINovelWriter.Shared.Models;

public class PromptFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next(context);
        Console.WriteLine(
            $"Prompt Rendered:\n----------------------------------------\n{context.RenderedPrompt}\n---------------------------------\n");
    }
}