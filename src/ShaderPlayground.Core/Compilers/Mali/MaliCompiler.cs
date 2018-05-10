using System;
using System.Collections.Generic;
using System.IO;
using ShaderPlayground.Core.Languages;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Mali
{
    internal sealed class MaliCompiler : IShaderCompiler
    {
        public string Name { get; } = "mali";

        public string DisplayName { get; } = "Mali offline compiler";

        public string Description { get; } = "ARM Mali offline compiler";

        public ShaderCompilerParameter[] Parameters { get; } = new ShaderCompilerParameter[0];

        public ShaderCompilerResult Compile(string code, Dictionary<string, string> arguments)
        {
            var stage = GetStageFlag(Validate.Option(arguments, "ShaderStage", GlslLanguage.ShaderStageOptions));

            using (var tempFile = TempFile.FromText(code))
            {
                ProcessHelper.Run(
                    Path.Combine(AppContext.BaseDirectory, "Binaries", "Mali", "malisc.exe"),
                    $"{stage} {tempFile.FilePath}",
                    out var stdOutput,
                    out var stdError);

                return new ShaderCompilerResult(
                    null, 
                    new ShaderCompilerOutput("Output", null, stdOutput));
            }
        }

        private static string GetStageFlag(string shaderStage)
        {
            switch (shaderStage)
            {
                case "vert":
                    return "--vertex";

                case "tesc":
                    return "--tessellation_control";

                case "tese":
                    return "--tessellation_evaluation";

                case "geom":
                    return "--geometry";

                case "frag":
                    return "--fragment";

                case "comp":
                    return "--compute";

                default:
                    throw new ArgumentOutOfRangeException(nameof(shaderStage));
            }
        }
    }
}
