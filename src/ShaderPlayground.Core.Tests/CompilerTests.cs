using System.Collections.Generic;
using Xunit;

namespace ShaderPlayground.Core.Tests
{
    public class CompilerTests
    {
        private const string HlslCode = @"struct PSInput
{
	float4 color : COLOR;
};

float4 PSMain(PSInput input) : SV_TARGET
{
	return input.color;
}";

        private const string WglsCode = @"[[location(0)]] var<out> gl_FragColor : vec4<f32>;
[[stage(fragment)]]
fn main() -> void {
    var color : vec4<f32> = vec4<f32>(0.1, 0.2, 0.3, 1.0);
    gl_FragColor = color;
    return;
}";

        [Fact]
        public void CanCompileHlslToDxbcUsingFxc()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Fxc,
                    new Dictionary<string, string>
                    {
                        { "EntryPoint", "PSMain" },
                        { "TargetProfile", "ps_5_0" },
                        { "DisableOptimizations", "false" },
                        { "OptimizationLevel", "2" },
                    }))[0];

            Assert.Equal("DXBC", result.Outputs[0].Language);
            Assert.Equal(713, result.Outputs[0].Value.Length);
        }

        [Fact]
        public void CanCompileHlslToDxilUsingDxc()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Dxc,
                    new Dictionary<string, string>
                    {
                        { "EntryPoint", "PSMain" },
                        { "TargetProfile", "ps_6_0" },
                        { "DisableOptimizations", "false" },
                        { "OptimizationLevel", "2" },
                        { "OutputLanguage", LanguageNames.Dxil },
                    }))[0];

            Assert.Equal("DXIL", result.PipeableOutput.Language);
            Assert.Equal(ShaderCodeType.Binary, result.PipeableOutput.CodeType);
            Assert.Equal(1620, result.PipeableOutput.Binary.Length);
            Assert.Equal("Disassembly", result.Outputs[0].DisplayName);
            Assert.Equal("DXIL", result.Outputs[0].Language);
            Assert.Equal(3835, result.Outputs[0].Value.Length);
            Assert.Equal("AST", result.Outputs[1].DisplayName);
            Assert.Equal(1070, result.Outputs[1].Value.Length);
        }

        [Fact]
        public void CanCompileHlslToSpirVUsingDxc()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Dxc,
                    new Dictionary<string, string>
                    {
                        { "EntryPoint", "PSMain" },
                        { "TargetProfile", "ps_6_0" },
                        { "DisableOptimizations", "false" },
                        { "OptimizationLevel", "2" },
                        { "OutputLanguage", LanguageNames.SpirV },
                    }))[0];

            Assert.Equal("SPIR-V", result.PipeableOutput.Language);
            Assert.Equal(ShaderCodeType.Binary, result.PipeableOutput.CodeType);
            Assert.Equal(368, result.PipeableOutput.Binary.Length);
            Assert.Equal("SPIR-V", result.Outputs[0].Language);
            Assert.Equal(1145, result.Outputs[0].Value.Length);
        }

        [Fact]
        public void CanCompileHlslToSpirVUsingGlslang()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Glslang,
                    new Dictionary<string, string>
                    {
                        { "ShaderStage", "frag" },
                        { "Target", "Vulkan 1.0" },
                        { "EntryPoint", "PSMain" }
                    }))[0];

            Assert.Equal("SPIR-V", result.Outputs[0].Language);
            Assert.Equal(1416, result.Outputs[0].Value.Length);
        }

        [Fact]
        public void CanPipeHlslToSpirVToMali()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Dxc,
                    new Dictionary<string, string>
                    {
                        { "EntryPoint", "PSMain" },
                        { "TargetProfile", "ps_6_0" },
                        { "OutputLanguage", LanguageNames.SpirV },
                        { "DisableOptimizations", "false" },
                        { "OptimizationLevel", "2" }
                    }),
                new CompilationStep(
                    CompilerNames.Mali,
                    new Dictionary<string, string>
                    {
                        { "ShaderStage", "frag" },
                        { "EntryPoint", "PSMain" },
                        { "Core", "Mali-G72" }
                    }))[1];

            Assert.Null(result.Outputs[0].Language);
            Assert.Equal(663, result.Outputs[0].Value.Length);
        }

        [Fact]
        public void CanCompileWsglToSpirv()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Wgsl, WglsCode),
                new CompilationStep(
                    CompilerNames.Tint,
                    new Dictionary<string, string>
                    {
                        { "ShaderStage", "fragment" },
                        { "OutputLanguage", LanguageNames.SpirV },
                        { "Version", "trunk" },
                    }))[0];

            Assert.True(result.Success);
        }
    }
}
