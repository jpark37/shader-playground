using System.Collections.Generic;
using ShaderPlayground.Core.Languages;
using Xunit;

namespace ShaderPlayground.Core.Tests
{
    public class CompilerTests
    {
        [Fact]
        public void CanCompileHlsl()
        {
            var hlslLanguage = new HlslLanguage();

            var result = Compiler.Compile(
                hlslLanguage.DefaultCode,
                hlslLanguage.Name,
                "fxc",
                new Dictionary<string, string>
                {
                    { "EntryPoint", "VSMain" },
                    { "TargetProfile", "vs_5_0" },
                    { "DisableOptimizations", "false" },
                    { "OptimizationLevel", "2" },
                });

            Assert.Equal("DXBC", result.Outputs[0].Language);
            Assert.Equal(947, result.Outputs[0].Value.Length);
        }
    }
}
