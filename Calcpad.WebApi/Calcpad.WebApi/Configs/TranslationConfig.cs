namespace Calcpad.WebApi.Configs
{
    public class TranslationConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether pure ASCII characters should be ignored during processing.
        /// </summary>
        public bool IgnorePureASCCI { get; set; }

        public string Prompt { get; set; } =
            """
                You are a professional translator. Translate the provided JSON array of strings to {lang} language.
                Keep the format as a JSON array of strings. Only translate the text content, do not translate any code, formulas, or special characters.
                Return the translated JSON array of strings.
                """;
    }
}
