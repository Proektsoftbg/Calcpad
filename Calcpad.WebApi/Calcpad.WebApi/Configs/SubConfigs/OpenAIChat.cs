namespace Calcpad.WebApi.Configs.SubConfigs
{
    /// <summary>
    /// OpenAI Chat configuration
    /// </summary>
    public class OpenAIChat
    {
        public string Endpoint { get; set; }

        public string Model { get; set; }

        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed length for a token.
        /// </summary>
        public long MaxTokenLenght { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Model) && !string.IsNullOrEmpty(ApiKey);
        }
    }
}
