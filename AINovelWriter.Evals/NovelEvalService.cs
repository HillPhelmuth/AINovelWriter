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
    private readonly List<string> _evals = [Gptcharacterdevelopment, Gptclarity, Gptcreativity, Gptengagement, Gptrelevance, Gptwritingdetail];
    private List<string> CriticalEvals => _evals.Select(x => $"{x}Critical").ToList();
    public NovelEvalService(Kernel kernel)
    {
        _kernel = kernel;

    }
    public async Task<List<ResultScore<CustomEvalOutput>>> ExecuteEvals(List<IInputModel> inputs)
    {
        var kernel = _kernel.Clone();
        var evalService = new EvalService(kernel);
        //var evalFunctions = GetEvalFunctions();
        //TEMP FOR TESTING
        var evalFunctions = GetCriticalEvalFunctions();
        foreach (var eval in evalFunctions)
        {
            //Console.WriteLine($"Adding eval function: {eval.Key} ({eval.Value.Name})");
            evalService.AddEvalFunction(eval.Key, eval.Value);
        }
        var resultScores = new List<ResultScore<CustomEvalOutput>>();
        var inputList = SplitInputList(inputs, 5);
        foreach (var inputGroup in inputList)
        {
            var tasks = new List<Task<ResultScore<CustomEvalOutput>>>();
            foreach (var input in inputGroup)
            {
                try
                {
                    // TEMP FOR TESTING
                    var result =
                        evalService.ExecuteEvalWithCustomOutput<CustomEvalOutput>(input,
                            nameof(CustomEvalOutput.Score).ToLower());
                    tasks.Add(result);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            var results = await Task.WhenAll(tasks);
            resultScores.AddRange(results);
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
            var function = kernel.CreateFunctionFromPromptYaml(yaml);
            evalFunctions[eval] = function;
        }

        return evalFunctions;
    }
    private List<List<IInputModel>> SplitInputList(List<IInputModel> inputs, int maxSize)
    {
        var result = new List<List<IInputModel>>();
        for (int i = 0; i < inputs.Count; i += maxSize)
        {
            result.Add(inputs.Skip(i).Take(maxSize).ToList());
        }
        return result;
    }
    private Dictionary<string, KernelFunction> GetCriticalEvalFunctions()
    {
        var kernel = _kernel.Clone();
        var evalFunctions = new Dictionary<string, KernelFunction>();
        foreach (var eval in CriticalEvals)
        {
            var yaml = ExtractFromAssembly<string>($"{eval}.yaml");
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
            //TEMP FOR TESTING
            ..CriticalEvals.Select(eval =>
                new NovelInputModel(eval, new KernelArguments { ["details"] = details, ["chapter"] = text, ["styleGuide"] = CondensedStyleGuide }))
        ];
    }
    public const string CondensedStyleGuide = """
                                                  * **Voice & Tone**
                                                    Maintain a consistent narrative register and vary sentence rhythms to match mood.
                                                  
                                                  * **Description & Immersion**
                                                    Use a few vivid sensory details per scene (sight, sound, touch) to ground the reader.
                                                  
                                                  * **Characters**
                                                    Keep voices and motivations consistent; highlight each with a unique quirk.
                                                  
                                                  * **Plot & Continuity**
                                                    Cross-check names, events, and rules against past text; ensure logical progression.
                                                  
                                                  * **Show, Don’t Tell**
                                                    Convey emotion through actions, body language, and setting cues.
                                                  
                                                  * **Dialogue**
                                                    Advance plot and reveal character via tension, implication, and subtext.
                                                  
                                                  * **Pacing**
                                                    Start with a hook, build to a turning point, end with a cliffhanger; balance action, thought, and talk.
                                                  
                                                  * **Action Scenes**
                                                    Choreograph each blow with vivid precision: detail swings, impacts, and weapon clashes.
                                                    Immerse the reader with sensory beats—the clang of metal, the thud of boots, the sting of sweat—while weaving in characters’ tactics and emotional stakes.
                                                  
                                                  """;

}

public class CustomEvalOutput
{
    public string MajorWeaknesses { get; set; }
    public string MinorWeaknesses { get; set; }
    public int Score { get; set; }
}
