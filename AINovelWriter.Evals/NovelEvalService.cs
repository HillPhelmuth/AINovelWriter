using System.Reflection;
using System.Text.Json;
using HillPhelmuth.SemanticKernel.LlmAsJudgeEvals;
using Microsoft.SemanticKernel;

namespace AINovelWriter.Evals;

public class NovelEvalService
{
    private const string Gptcharacterdevelopment = "GptCharacterDevelopment";
    private const string Gptclarity = "GptClarity";
    private const string Gptcreativity = "GptCreativity";
    private const string Gptengagement = "GptEngagement";
    private const string Gptrelevance = "GptRelevance";
    private const string Gptwritingdetail = "GptWritingDetail";
    private readonly Kernel _kernel;
    private readonly List<string> _evals = [Gptcharacterdevelopment, Gptclarity, Gptcreativity, Gptengagement,Gptrelevance, Gptwritingdetail];

    public NovelEvalService(Kernel kernel)
    {
        _kernel = kernel;
        
    }
    public async Task<List<ResultScore>> ExecuteEvals(List<IInputModel> inputs)
    {
        var kernel = _kernel.Clone();
        var evalService = new EvalService(kernel);
        var evalFunctions = GetEvalFunctions();
        foreach (var eval in evalFunctions)
        {
            evalService.AddEvalFunction(eval.Key, eval.Value);
        }
        var resultScores = new List<ResultScore>();
        foreach (var input in inputs)
        {
	        try
	        {
                var result = await evalService.ExecuteScorePlusEval(input);
		        resultScores.Add(result);
	        }
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
        return resultScores;
    }
    private Dictionary<string, KernelFunction> GetEvalFunctions()
    {
        var kernel = _kernel.Clone();
        var evalFunctions = new Dictionary<string, KernelFunction>();
        foreach (var eval in _evals)
        {
            var yaml = ExtractFromAssembly<string>($"{eval}.yaml");
            var formatAppend = """
                                   ### Output Format
                                   Output must be in the following json format
                                   ```json
                                   {
                                     "thoughtChain": "Let's think step by step: [Step-by-step explanation of strenghts and weakness (must include a sentance for each)]",
                                     "explanation": "[Reasons for the rating score based on strengths and weaknesses]",
                                     "score": [number 1 - 5]
                                   }
                                   ```
                               """;
            yaml = $"{yaml}\n\n{formatAppend}";
            var function = kernel.CreateFunctionFromPromptYaml(yaml);
            evalFunctions[eval] = function;
        }
        
        return evalFunctions;
    }
    internal static T ExtractFromAssembly<T>(string fileName)
    {
	    var assembly = Assembly.GetExecutingAssembly();
	    var jsonName = assembly.GetManifestResourceNames()
		    .SingleOrDefault(s => s.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) ?? "";
	    using var stream = assembly.GetManifestResourceStream(jsonName);
	    using var reader = new StreamReader(stream);
	    object result = reader.ReadToEnd();
	    if (typeof(T) == typeof(string))
		    return (T)result;
	    return JsonSerializer.Deserialize<T>(result.ToString());
    }
	public List<IInputModel> CreateInputModels(string text, string details)
    {
        return
        [
            .._evals.Select(eval =>
                new NovelInputModel(eval, new KernelArguments { ["details"] = details, ["chapter"] = text }))
        ];
    }
    
}