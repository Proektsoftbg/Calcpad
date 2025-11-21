using Calcpad.WebApi.Models.Base;

namespace Calcpad.WebApi.Models
{
    /// <summary>
    /// Represents an AI prompt definition, including its unique key and prompt text.
    /// </summary>
    public class AIPromptModel : MongoDoc
    {
        /// <summary>
        /// Gets or sets a value indicating whether the setting was loaded from a configuration file.
        /// if true, the prompt can be modified from config file
        /// </summary>
        public bool IsConfig { get; set; }

        public string Name { get; set; }

        public string Prompt { get; set; }
    }
}
