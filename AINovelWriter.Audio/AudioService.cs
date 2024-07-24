using Microsoft.JSInterop;

namespace AINovelWriter.Audio
{
    public class AudioService(IJSRuntime jsRuntime) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/AINovelWriter.Audio/audioInterop.js").AsTask());

        /// <summary>
        /// Initializes the audio service with the specified element ID.
        /// </summary>
        /// <param name="elementId">The audio element.</param>
        /// <param name="url">base64 url</param>
        public async ValueTask Init(string elementId, string url)
        {
            await (await moduleTask.Value).InvokeVoidAsync("init", elementId, url);
        }

        /// <summary>
        /// Gets the progress of the audio playback.
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns>The progress of the audio playback as a double value.</returns>
        public async ValueTask<double> GetProgress(string elementId)
        {
            return await (await moduleTask.Value).InvokeAsync<double>("getProgress", elementId);
        }

        /// <summary>
        /// Gets the current time of the audio playback.
        /// </summary>
        /// <returns>The current time of the audio playback as a double value.</returns>
        public async ValueTask<double> GetCurrentTime(string elementId)
        {
            return await (await moduleTask.Value).InvokeAsync<double>("getCurrentTime", elementId);
        }

        /// <summary>
        /// Gets the duration of the audio.
        /// </summary>
        /// <returns>The duration of the audio as a double value.</returns>
        public async ValueTask<double> GetDuration(string elementId)
        {
            return await (await moduleTask.Value).InvokeAsync<double>("getDuration", elementId);
        }

        /// <summary>
        /// Appends the audio buffer with the specified base64 string.
        /// </summary>
        /// <param name="base64String">The base64 string representing the audio buffer.</param>
        public async ValueTask AppendBuffer(string base64String)
        {
            await (await moduleTask.Value).InvokeVoidAsync("appendBuffer", base64String);
        }

        /// <summary>
        /// Signals the end of the audio stream.
        /// </summary>
        public async ValueTask EndOfStream()
        {
            await (await moduleTask.Value).InvokeVoidAsync("endOfStream");
        }

        /// <summary>
        /// Starts playing the audio.
        /// </summary>
        public async ValueTask Play(string elementId)
        {
            await (await moduleTask.Value).InvokeVoidAsync("play", elementId);
        }

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        public async ValueTask Pause(string elementId)
        {
            await (await moduleTask.Value).InvokeVoidAsync("pause", elementId);
        }

        /// <summary>
        /// Changes the progress of the audio playback to the specified value.
        /// </summary>
        /// <param name="value">The new progress value.</param>
        /// <param name="elementId"></param>
        public async ValueTask ChangeProgress(double value, string elementId)
        {
            await (await moduleTask.Value).InvokeVoidAsync("changeProgress", value, elementId);
        }
        public async ValueTask DownloadFile(string fileName, byte[] fileData)
        {
            await (await moduleTask.Value).InvokeVoidAsync("downloadFile", fileName, fileData);
        }
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
