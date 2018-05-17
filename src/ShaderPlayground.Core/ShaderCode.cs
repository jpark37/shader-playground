namespace ShaderPlayground.Core
{
    public sealed class ShaderCode
    {
        public string Language { get; }

        public ShaderCodeType CodeType { get; }

        // Only one of the following properties should be non-null.
        public string Text { get; }
        public byte[] Binary { get; }

        public ShaderCode(string language, string text)
        {
            Language = language;
            CodeType = ShaderCodeType.Text;
            Text = text;
        }

        public ShaderCode(string language, byte[] binary)
        {
            Language = language;
            CodeType = ShaderCodeType.Binary;
            Binary = binary;
        }
    }

    public enum ShaderCodeType
    {
        Text,
        Binary
    }
}
