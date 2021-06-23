namespace ShaderPlayground.Core.Languages
{
    internal sealed class MetalLanguage : IShaderLanguage
    {
        public string Name { get; } = LanguageNames.Metal;

        public string DefaultCode { get; } = DefaultMetalCode;

        public string FileExtension { get; } = "metal";

        private static readonly string DefaultMetalCode = @"#include <metal_stdlib>
#include <simd/simd.h>

using namespace metal;

struct PSInput
{
    float4 color [[user(locn0)]];
};

struct PSOutput
{
    float4 color [[color(0)]];
};

fragment PSOutput PSMain(PSInput input [[stage_in]])
{
    PSOutput output = {};
    output.color = input.color;
    return output;
}";
    }
}
