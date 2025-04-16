using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AINovelWriter.Shared.Models;

public sealed class LoggingHandler(HttpMessageHandler innerHandler, ILoggerFactory output) : DelegatingHandler(innerHandler)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    private readonly ILogger<LoggingHandler> _output = output.CreateLogger<LoggingHandler>();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        
        _output.LogInformation(request.RequestUri?.ToString());
        var isStream = false;
        if (request.Content is not null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            _output.LogInformation("=== REQUEST ===");
            try
            {
                var root = JsonNode.Parse(requestBody);
                if (root != null)
                {
                    isStream = root["stream"]?.GetValue<bool>() ?? false;
                }
                string formattedContent = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(requestBody), s_jsonSerializerOptions);
                _output.LogInformation(formattedContent);
            }
            catch (JsonException)
            {
                _output.LogInformation(requestBody);
            }
            _output.LogInformation(string.Empty);
        }

        // Call the next handler in the pipeline
        var responseMessage = await base.SendAsync(request, cancellationToken);

        if (isStream) return responseMessage;
        var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        _output.LogInformation("=== RESPONSE ===");
        _output.LogInformation(responseBody);
        _output.LogInformation(string.Empty);
        return responseMessage;

       
    }
}