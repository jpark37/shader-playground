using System;
using System.IO;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.SpirVCrossIspc
{
    internal sealed class SpirVCrossIspcCompiler : IShaderCompiler
    {
        public string Name { get; } = CompilerNames.SpirVCrossIspc;
        public string DisplayName { get; } = "SPIRV-Cross (ISPC)";
        public string Description { get; } = "Intel fork of SPIRV-Cross with support for compiling to ISPC";

        public string[] InputLanguages { get; } = { LanguageNames.SpirV };

        public ShaderCompilerParameter[] Parameters { get; } =
        {
            CommonParameters.CreateVersionParameter("spirv-cross-ispc"),
            CommonParameters.CreateOutputParameter(new[] { LanguageNames.Ispc })
        };

        public ShaderCompilerResult Compile(ShaderCode shaderCode, ShaderCompilerArguments arguments)
        {
            var args = "--ispc";

            using (var tempFile = TempFile.FromShaderCode(shaderCode))
            {
                var outputPath = $"{tempFile.FilePath}.out";

                ProcessHelper.Run(
                    CommonParameters.GetBinaryPath("spirv-cross-ispc", arguments, "spirv-cross.exe"),
                    $"--output \"{outputPath}\" \"{tempFile.FilePath}\" {args}",
                    out var _,
                    out var stdError);

                var hasCompilationErrors = !string.IsNullOrWhiteSpace(stdError);

                var textOutput = FileHelper.ReadAllTextIfExists(outputPath);

                FileHelper.DeleteIfExists(outputPath);

                return new ShaderCompilerResult(
                    !hasCompilationErrors,
                    new ShaderCode(LanguageNames.Ispc, textOutput),
                    hasCompilationErrors ? (int?) 1 : null,
                    new ShaderCompilerOutput("Output", LanguageNames.Ispc, textOutput),
                    new ShaderCompilerOutput("Errors", null, hasCompilationErrors ? stdError : "<No compilation errors>"));
            }
        }
    }
}
