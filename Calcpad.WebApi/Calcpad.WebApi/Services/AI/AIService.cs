using System.ClientModel;
using System.Text.Json;
using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Utils.Json;
using Calcpad.WebApi.Utils.Web.Service;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

namespace Calcpad.WebApi.Services.AI
{
    public class AIService(
        AppSettings<AIConfig> aiConfig,
        AppSettings<TranslationConfig> transConfig,
        OpenAIChatClient chatClient
    ) : IScopedService
    {
        public bool IsEnabled => aiConfig.Value.Enable;

        public async Task<List<Tuple<string, string>>> TranslateToLang(
            List<string> contents,
            string lang
        )
        {
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
            var maxTokensPerChunk = maxTokenLength / 2 - EstimateTokens(systemPrompt);

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
                var jsonString = JsonSerializer.Serialize(chunk);
                var chatResponse = await chatClient.GetResponseAsync(
                    [new(ChatRole.System, systemPrompt), new(ChatRole.User, jsonString)]
                );
                var responseText = chatResponse.Messages.Last().Text ?? string.Empty;
                // 只获取 ```\s+json ... ``` 之间的内容
                var codeBlockStart = responseText.IndexOf("```");
                var codeBlockLangStart = responseText.IndexOf('\n', codeBlockStart) + 1;
                var codeBlockEnd = responseText.IndexOf("```", codeBlockLangStart);
                var jsonContent = responseText[codeBlockLangStart..codeBlockEnd].Trim();
                var translationResults = jsonContent.JsonTo<List<string>>();
                if (translationResults == null || translationResults.Count != chunk.Count)
                {
                    throw new InvalidOperationException("Translation result is invalid.");
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
