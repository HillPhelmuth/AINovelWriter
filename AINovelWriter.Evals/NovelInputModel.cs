using HillPhelmuth.SemanticKernel.LlmAsJudgeEvals;
using Microsoft.SemanticKernel;


namespace AINovelWriter.Evals;

public class NovelInputModel(string functionName, KernelArguments requiredInputs) : IInputModel
{
    public string FunctionName { get; } = functionName;
    public KernelArguments RequiredInputs { get; } = requiredInputs;
}