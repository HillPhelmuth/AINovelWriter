using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace AINovelWriter.Shared.Models;

public class SystemToDeveloperRoleHandler(HttpMessageHandler innerHandler, ILoggerFactory output) : DelegatingHandler(innerHandler)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    private readonly ILogger<SystemToDeveloperRoleHandler> _output = output.CreateLogger<SystemToDeveloperRoleHandler>();
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Only handle if there is content and it's JSON.
        if (request.Content is not { Headers.ContentType.MediaType: "application/json" })
            return await base.SendAsync(request, cancellationToken);
        var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        this._output.LogInformation("=== Original Request ===");
        this._output.LogInformation(requestBody);
        this._output.LogInformation(string.Empty);
        if (string.IsNullOrWhiteSpace(requestBody)) return await base.SendAsync(request, cancellationToken);
        var isStreaming = false;
        try
        {
            var root = JsonNode.Parse(requestBody);
            
            if (root is not null)
            {
                var modelValue = root["model"]?.GetValue<string>() ?? string.Empty;
                List<string> keysToRemove = ["top_p", "presence_penalty", "frequency_penalty", "logprobs", "top_logprobs", "logit_bias"];
                if (modelValue.Contains("o3"))
                {
                    keysToRemove.Add("temperature");
                }
                foreach (var key in keysToRemove)
                {
                    if (root is JsonObject obj)
                    {
                        obj.Remove(key);
                    }
                }
                isStreaming = root["stream"]?.GetValue<bool>() ?? false;
                var isO1Mini = modelValue.Contains("o1-mini");
                var messages = root["messages"]?.AsArray();
                if (messages is not null)
                {
                    foreach (var message in messages)
                    {
                        if (message?["role"]?.GetValue<string>() == "system")
                        {
                            message["role"] = isO1Mini ? "user" : "developer";
                        }
                    }

                    var modifiedJson = root.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    this._output.LogInformation("=== Modified Request ===");
                    this._output.LogInformation(modifiedJson);
                    this._output.LogInformation(string.Empty);
                    request.Content = new StringContent(modifiedJson, Encoding.UTF8, "application/json");
                }
            }
        }
        catch
        {
            // If parsing fails, do nothing – just fall through
            // and send the original request body.
        }

        var responseMessage = await base.SendAsync(request, cancellationToken);
        if (!isStreaming)
        {
            var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            this._output.LogInformation("=== RESPONSE ===");
            this._output.LogInformation(responseBody);
            this._output.LogInformation(string.Empty);
        }
        return responseMessage;
    }
}