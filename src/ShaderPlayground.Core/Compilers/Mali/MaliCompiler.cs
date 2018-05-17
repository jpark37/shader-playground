using System;
using System.IO;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Mali
{
    internal sealed class MaliCompiler : IShaderCompiler
    {
        public string Name { get; } = CompilerNames.Mali;
        public string DisplayName { get; } = "Mali offline compiler";
        public string Description { get; } = "ARM Mali offline compiler";

        public string[] InputLanguages { get; } = { LanguageNames.Glsl, LanguageNames.SpirV };

        public ShaderCompilerParameter[] Parameters { get; } = new[]
        {
            CommonParameters.GlslShaderStage,
            CommonParameters.SpirVEntryPoint
        };

        // TODO: Driver, core, revision parameters

        public ShaderCompilerResult Compile(ShaderCode shaderCode, ShaderCompilerArguments arguments)
        {
            var stage = GetStageFlag(arguments.GetString("ShaderStage"));

            var args = string.Empty;
            if (shaderCode.Language == LanguageNames.SpirV)
            {
                var spirVEntryPoint = arguments.GetString("EntryPoint");

                args += "--spirv ";
                args += $"--spirv_entrypoint_name {spirVEntryPoint}";
            }

            using (var tempFile = TempFile.FromShaderCode(shaderCode))
            {
                ProcessHelper.Run(
                    Path.Combine(AppContext.BaseDirectory, "Binaries", "Mali", "malisc.exe"),
                    $"{stage} {args} {tempFile.FilePath}",
                    out var stdOutput,
                    out var stdError);

                return new ShaderCompilerResult(
                    null,
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
