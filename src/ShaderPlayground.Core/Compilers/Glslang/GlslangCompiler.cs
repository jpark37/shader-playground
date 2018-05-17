using System;
using System.IO;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Glslang
{
    internal sealed class GlslangCompiler : IShaderCompiler
    {
        public string Name { get; } = CompilerNames.Glslang;
        public string DisplayName { get; } = "glslang";
        public string Description { get; } = "Khronos glslangvalidator.exe";

        public string[] InputLanguages { get; } = 
        {
            LanguageNames.Glsl,
            LanguageNames.Hlsl
        };

        public ShaderCompilerParameter[] Parameters { get; } =
        {
            CommonParameters.GlslShaderStage,
            new ShaderCompilerParameter("Target", "Target", ShaderCompilerParameterType.ComboBox, TargetOptions, SpirVVulkan1_0),
            CommonParameters.HlslEntryPoint, // TODO: Only visible when input language is HLSL?
            CommonParameters.CreateOutputParameter(new[] { LanguageNames.SpirV })
        };

        private const string SpirVVulkan1_0 = "Vulkan 1.0";
        private const string SpirVVulkan1_1 = "Vulkan 1.1";
        private const string SpirVOpenGL = "OpenGL";

        private static readonly string[] TargetOptions =
        {
            SpirVVulkan1_0,
            SpirVVulkan1_1,
            SpirVOpenGL
        };

        public ShaderCompilerResult Compile(ShaderCode shaderCode, ShaderCompilerArguments arguments)
        {
            var stage = arguments.GetString("ShaderStage");

            var target = arguments.GetString("Target");
            var targetOption = string.Empty;
            switch (target)
            {
                case SpirVVulkan1_0:
                    targetOption = "--target-env vulkan1.0";
                    break;

                case SpirVVulkan1_1:
                    targetOption = "--target-env vulkan1.1";
                    break;

                case SpirVOpenGL:
                    targetOption = "--target-env opengl";
                    break;
            }

            if (shaderCode.Language == LanguageNames.Hlsl)
            {
                targetOption += " -D";
                targetOption += $" -e {arguments.GetString("EntryPoint")}";
            }

            using (var tempFile = TempFile.FromShaderCode(shaderCode))
            {
                var binaryPath = $"{tempFile.FilePath}.o";

                var validationErrors = RunGlslValidator(stage, tempFile, targetOption);
                var spirv = RunGlslValidator(stage, tempFile, targetOption + $" -H -o \"{binaryPath}\"");
                var ast = RunGlslValidator(stage, tempFile, targetOption + " -i");

                var binaryOutput = FileHelper.ReadAllBytesIfExists(binaryPath);

                FileHelper.DeleteIfExists(binaryPath);

                var hasValidationErrors = !string.IsNullOrWhiteSpace(validationErrors);

                return new ShaderCompilerResult(
                    new ShaderCode(LanguageNames.SpirV, binaryOutput),
                    hasValidationErrors ? 2 : (int?) null,
                    new ShaderCompilerOutput("Disassembly", LanguageNames.SpirV, spirv),
                    new ShaderCompilerOutput("AST", null, ast),
                    new ShaderCompilerOutput("Validation", null, hasValidationErrors ? validationErrors : "<No validation errors>"));
            }
        }

        private static string RunGlslValidator(string stage, string codeFilePath, string arguments)
        {
            ProcessHelper.Run(
                Path.Combine(AppContext.BaseDirectory, "Binaries", "Glslang", "glslangValidator.exe"),
                $"-S {stage} -d {arguments} {codeFilePath}",
                out var stdOutput,
                out var stdError);

            // First line is always full path of input file - remove that.
            string RemoveCodeFilePath(string value)
            {
                return (value != null && value.StartsWith(codeFilePath))
                    ? value.Substring(codeFilePath.Length).Trim()
                    : value;
            }

            stdOutput = RemoveCodeFilePath(stdOutput);
            stdError = RemoveCodeFilePath(stdError);

            if (!string.IsNullOrWhiteSpace(stdError))
            {
                return stdError;
            }

            if (!string.IsNullOrWhiteSpace(stdOutput))
            {
                return stdOutput;
            }

            return null;
        }
    }
}
