using System.ClientModel;
using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Utils.Web.Service;
using Microsoft.Extensions.AI;

namespace Calcpad.WebApi.Services.AI
{
    /// <summary>
    /// OpenAI Chat Client wrapper for IChatClient interface
    /// Singleton service
    /// </summary>
    public class OpenAIChatClient : IChatClient, ISingletonService
    {
        private IChatClient? _chatClient;

        public long MaxTokenLength { get; private set; } = 0;

        public OpenAIChatClient(AppSettings<AIConfig> config)
        {
            var openAiChat = config.Value.OpenAIChat;
            if (openAiChat == null || !openAiChat.IsValid())
                return;

            var credentials = new ApiKeyCredential(openAiChat.ApiKey);
            var options = new OpenAI.OpenAIClientOptions();
            if (!string.IsNullOrEmpty(openAiChat.Endpoint))
            {
                options.Endpoint = new Uri(openAiChat.Endpoint);
            }
            _chatClient = new OpenAI.Chat.ChatClient(
                openAiChat.Model,
                credentials,
                options
            ).AsIChatClient();
            MaxTokenLength = openAiChat.MaxTokenLenght;
        }

        #region IChatClient
        public void Dispose()
        {
            _chatClient?.Dispose();
        }

        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            if (_chatClient == null)
            {
                throw new InvalidOperationException("OpenAIChatClient is not initialized.");
            }
            return await _chatClient.GetResponseAsync(messages, options, cancellationToken);
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            if (_chatClient == null)
            {
                throw new InvalidOperationException("OpenAIChatClient is not initialized.");
            }

            return _chatClient.GetService(serviceType, serviceKey);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            if (_chatClient == null)
            {
                throw new InvalidOperationException("OpenAIChatClient is not initialized.");
            }

            return _chatClient.GetStreamingResponseAsync(messages, options, cancellationToken);
        }
        #endregion
    }
}
