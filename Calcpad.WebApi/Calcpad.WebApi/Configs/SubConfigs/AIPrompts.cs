namespace Calcpad.WebApi.Configs.SubConfigs
{
    public class AIPrompts
    {
        public string Name { get; set; }
        public string Prompt { get; set; }

        /// <summary>
        /// Gets the resource key for the "TranslateCpd" string.
        /// </summary>
        public static string TranslateCpd => "TranslateCpd";
    }
}
