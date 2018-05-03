using OnlineShaderCompiler.Framework.Processors.Dxc;
using OnlineShaderCompiler.Framework.Processors.Fxc;

namespace OnlineShaderCompiler.Framework.Languages
{
    public sealed class HlslLanguage : IShaderLanguage
    {
        public string Name { get; } = "HLSL";

        public string DefaultCode { get; } = DefaultHlslCode;

        public ShaderProcessorParameter[] LanguageParameters { get; } = new[]
        {
            new ShaderProcessorParameter("EntryPoint", "Entry point", ShaderProcessorParameterType.TextBox, defaultValue: "VSMain")
        };

        public IShaderProcessor[] Processors { get; } = new IShaderProcessor[]
        {
            new DxcProcessor(),
            new FxcProcessor()
        };

        private static readonly string DefaultHlslCode = @"struct PSInput
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

PSInput VSMain(float4 position : POSITION, float4 color : COLOR)
{
	PSInput result;

	result.position = position;
	result.color = color;

	return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
	return input.color;
}";
    }
}
