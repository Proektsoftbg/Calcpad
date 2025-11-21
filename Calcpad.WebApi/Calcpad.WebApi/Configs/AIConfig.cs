using Calcpad.WebApi.Configs.SubConfigs;

namespace Calcpad.WebApi.Configs
{
    public class AIConfig
    {
        public bool Enable { get; set; } = false;

        /// <summary>
        /// chat config for openai chat api
        /// </summary>
        public OpenAIChat? OpenAIChat { get; set; }

        /// <summary>
        /// Gets or sets the collection of AI prompt templates used by the application.
        /// </summary>
        public List<AIPrompts>? Prompts { get; set; }
    }
}
