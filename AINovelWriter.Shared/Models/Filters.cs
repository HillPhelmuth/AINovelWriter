using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    public event Action<AutoFunctionInvocationContext>? AutoFunctionInvoked;
    public event Action<AutoFunctionInvocationContext>? AutoFunctionCompleted;
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"Auto Function Invoked:\n--------------------------------------\n{context.Function.Name}\n---------------------------------\n");
        AutoFunctionInvoked?.Invoke(context);
        await next(context);
        AutoFunctionCompleted?.Invoke(context);

    }
}

public class FunctionFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        
        var functionName = context.Function.Name;
        if (functionName == "ExpandChapterOutline")
            Console.WriteLine($"Function Invoked: {functionName}, Args:\n--------------------------------------\n{JsonSerializer.Serialize(context.Arguments, new JsonSerializerOptions(){WriteIndented = true})}\n---------------------------------\n");
        await next(context);
        if (functionName == "ExpandChapterOutline")
        {
            Console.WriteLine($"Function Result:\n--------------------------------------\n{context.Result}\n---------------------------------\n");
        }
    }
}