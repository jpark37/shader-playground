using System;
using System.IO;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.SpirVCross
{
    internal sealed class SpirVCrossCompiler : IShaderCompiler
    {
        public string Name { get; } = CompilerNames.SpirVCross;
        public string DisplayName { get; } = "SPIRV-Cross";
        public string Description { get; } = "Khronos spirv-cross.exe";

        public string[] InputLanguages { get; } = { LanguageNames.SpirV };

        public ShaderCompilerParameter[] Parameters { get; } =
        {
            CommonParameters.CreateVersionParameter("spirv-cross"),
            //CommonParameters.GlslShaderStage,
            CommonParameters.HlslEntryPoint, // TODO: Only visible when input language is HLSL?
            CommonParameters.CreateOutputParameter(new[] { LanguageNames.Glsl, LanguageNames.Metal, LanguageNames.Hlsl, LanguageNames.Cpp })
        };

        public ShaderCompilerResult Compile(ShaderCode shaderCode, ShaderCompilerArguments arguments)
        {
            var args = string.Empty;

            var outputLanguage = arguments.GetString(CommonParameters.OutputLanguageParameterName);
            switch (outputLanguage)
            {
                case LanguageNames.Glsl:
                    args += ""; // TODO
                    break;

                case LanguageNames.Metal:
                    args += " --msl";
                    break;

                case LanguageNames.Hlsl:
                    args += " --hlsl";
                    break;

                case LanguageNames.Cpp:
                    args += " --cpp";
                    break;
            }

            args += $" --entry {arguments.GetString("EntryPoint")}";

            using (var tempFile = TempFile.FromShaderCode(shaderCode))
            {
                var outputPath = $"{tempFile.FilePath}.out";

                ProcessHelper.Run(
                    CommonParameters.GetBinaryPath("spirv-cross", arguments, "spirv-cross.exe"),
                    $"--output \"{outputPath}\" \"{tempFile.FilePath}\" {args}",
                    out var _,
                    out var stdError);

                var hasCompilationErrors = !string.IsNullOrWhiteSpace(stdError);

                var textOutput = FileHelper.ReadAllTextIfExists(outputPath);

                FileHelper.DeleteIfExists(outputPath);

                return new ShaderCompilerResult(
                    !hasCompilationErrors,
                    new ShaderCode(outputLanguage, textOutput),
                    hasCompilationErrors ? (int?) 1 : null,
                    new ShaderCompilerOutput("Output", outputLanguage, textOutput),
                    new ShaderCompilerOutput("Errors", null, hasCompilationErrors ? stdError : "<No compilation errors>"));
            }
        }
    }
}
