using Microsoft.SemanticKernel;
using PromptFlowEvalsAsPlugins;

namespace AINovelWriter.Evals;

public class NovelInputModel(string functionName, KernelArguments requiredInputs) : IInputModel
{
    public string FunctionName { get; } = functionName;
    public KernelArguments RequiredInputs { get; } = requiredInputs;
}