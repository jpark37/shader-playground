namespace ShaderPlayground.Core
{
    public sealed class ShaderCompilerOutput
    {
        public string DisplayName { get; }
        public string Language { get; }
        public string Value { get; }

        public ShaderCompilerOutput(string displayName, string language, string value)
        {
            DisplayName = displayName;
            Language = language;
            Value = value;
        }
    }
}
