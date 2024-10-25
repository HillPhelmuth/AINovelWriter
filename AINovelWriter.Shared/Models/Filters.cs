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

public class AutoFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"Auto Function Invoked:\n--------------------------------------\n{context.Function.Name}\n---------------------------------\n");
        await next(context);

    }
}