namespace ShaderPlayground.Core.Languages
{
    internal sealed class HlslLanguage : IShaderLanguage
    {
        public string Name { get; } = LanguageNames.Hlsl;

        public string DefaultCode { get; } = DefaultHlslCode;

        public string FileExtension { get; } = "hlsl";

        private static readonly string DefaultHlslCode = @"struct PSInput
{
	float4 color : COLOR;
};

float4 PSMain(PSInput input) : SV_TARGET
{
	return input.color;
}";
    }
}
