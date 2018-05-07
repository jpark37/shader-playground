using ShaderPlayground.Core.Compilers.Dxc;
using ShaderPlayground.Core.Compilers.Fxc;

namespace ShaderPlayground.Core.Languages
{
    public sealed class HlslLanguage : IShaderLanguage
    {
        public string Name { get; } = "HLSL";

        public string DefaultCode { get; } = DefaultHlslCode;

        public ShaderCompilerParameter[] LanguageParameters { get; } = new[]
        {
            new ShaderCompilerParameter("EntryPoint", "Entry point", ShaderCompilerParameterType.TextBox, defaultValue: "VSMain")
        };

        public IShaderCompiler[] Compilers { get; } = new IShaderCompiler[]
        {
            new DxcCompiler(),
            new FxcCompiler()
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
