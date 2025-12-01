using System.Text.Json;
using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Utils.Json;
using Calcpad.WebApi.Utils.Web.Exceptions;
using Calcpad.WebApi.Utils.Web.Service;
using Microsoft.Extensions.AI;

namespace Calcpad.WebApi.Services.AI
{
    public class AIService(
        AppSettings<AIConfig> aiConfig,
        AppSettings<TranslationConfig> transConfig,
        OpenAIChatClient chatClient,
        ILogger<AIService> logger
    ) : IScopedService
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions =
            new()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

        public bool IsEnabled => aiConfig.Value.Enable;

        /// <summary>
        /// Translates a list of text strings into the specified target language asynchronously.
        /// </summary>
        /// <remarks>The method processes the input texts in batches to optimize translation requests. The order of the
        /// returned tuples corresponds to the order of the unique input strings. If the translation service is disabled or not
        /// properly configured, the method returns an empty list without performing any translation.</remarks>
        /// <param name="contents">The list of text strings to translate. Duplicate entries are translated only once.</param>
        /// <param name="lang">The language code representing the target language for translation (for example, "en" for English or "zh" for
        /// Chinese).</param>
        /// <returns>A list of tuples where each tuple contains the original text and its translated equivalent. Returns an empty list if
        /// translation is not enabled or the translation service is unavailable.</returns>
        /// <exception cref="KnownException">Thrown if the translation service returns an invalid or malformed result.</exception>
        public async Task<List<Tuple<string, string>>> TranslateToLang(
            List<string> contents,
            string lang
        )
        {
            if (!IsEnabled)
                return [];

            var openAiChat = aiConfig.Value.OpenAIChat;
            if (openAiChat == null || !openAiChat.IsValid())
                return [];

            var systemPrompt = transConfig.Value.Prompt.Replace("{lang}", lang);

            // estimate token count
            // english approx 4 chars per token
            // chinese approx 1.5 chars per token
            // other approx 2 chars per token
            static int EstimateTokens(string text)
            {
                int englishCount = 0,
                    otherCount = 0;
                foreach (char c in text)
                {
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                        englishCount++;
                    else
                        otherCount++;
                }
                return (int)(englishCount / 4.0 + otherCount / 2.0);
            }

            var maxTokenLength = chatClient.MaxTokenLength;
            // reserve tokens for system prompt and response
            var maxTokensPerChunk = maxTokenLength / 3 - EstimateTokens(systemPrompt);

            var distinctContents = contents.Distinct().ToList();
            // split contents into chunks
            var chunks = new List<List<string>>();
            var currentChunk = new List<string>();
            var currentTokens = 0;
            foreach (var content in distinctContents)
            {
                var tokens = EstimateTokens(content);
                if (currentTokens + tokens > maxTokensPerChunk && currentChunk.Count > 0)
                {
                    chunks.Add(currentChunk);
                    currentChunk = [];
                    currentTokens = 0;
                }
                currentChunk.Add(content);
                currentTokens += tokens;
            }
            if (currentChunk.Count > 0)
            {
                chunks.Add(currentChunk);
            }

            // handle each chunk parallel
            var tasks = chunks.Select(async chunk =>
            {
                var jsonString = JsonSerializer.Serialize(chunk, _jsonSerializerOptions);
                var chatResponse = await chatClient.GetResponseAsync(
                    [new(ChatRole.System, systemPrompt), new(ChatRole.User, jsonString)]
                );
                var responseText = chatResponse.Messages.Last()?.Text ?? string.Empty;
                responseText = responseText.Trim();

                logger.LogDebug("Translation chunk response: {responseText}", responseText);

                // 只获取 ```\s+json ... ``` 之间的内容
                var codeBlockStart = responseText.IndexOf("```");

                var jsonContent = string.Empty;
                if (codeBlockStart < 0)
                {
                    // may be no code block, check if the whole response is json
                    if (responseText.StartsWith('[') && responseText.EndsWith(']'))
                    {
                        jsonContent = responseText;
                    }
                    else
                    {
                        throw new KnownException("Translation result does not contain code block.");
                    }
                }
                else
                {
                    var codeBlockLangStart = responseText.IndexOf('\n', codeBlockStart) + 1;
                    var codeBlockEnd = responseText.IndexOf("```", codeBlockLangStart);
                    jsonContent = responseText[codeBlockLangStart..codeBlockEnd].Trim();
                }

                var translationResults = jsonContent.JsonTo<List<string>>();
                if (translationResults == null || translationResults.Count != chunk.Count)
                {
                    throw new KnownException("Translation result is invalid.");
                }
                return chunk
                    .Zip(translationResults, (key, value) => Tuple.Create(key, value))
                    .ToList();
            });

            var resultsList = await Task.WhenAll(tasks);
            return [.. resultsList.SelectMany(x => x)];
        }
    }
}
