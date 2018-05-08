using System;
using System.Collections.Generic;
using System.IO;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Glslang
{
    public sealed class GlslangGlslCompiler : IShaderCompiler
    {
        public string Name { get; } = "glslang";
        public string DisplayName { get; } = "glslang";
        public string Description { get; } = "Khronos glslangvalidator.exe";

        public ShaderCompilerParameter[] Parameters { get; } =
        {
            new ShaderCompilerParameter("Target", "Target", ShaderCompilerParameterType.ComboBox, TargetOptions, ValidationOnly)
        };

        private const string ValidationOnly = "Validation only";
        private const string SpirVVulkan1_0 = "SPIR-V (Vulkan 1.0)";
        private const string SpirVVulkan1_1 = "SPIR-V (Vulkan 1.1)";
        private const string SpirVOpenGL = "SPIR-V (OpenGL)";

        private static readonly string[] TargetOptions =
        {
            ValidationOnly,
            SpirVVulkan1_0,
            SpirVVulkan1_1,
            SpirVOpenGL
        };

        public ShaderCompilerResult Compile(string code, Dictionary<string, string> arguments)
        {
            var stage = arguments["ShaderStage"];

            var target = arguments["Target"];
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

            using (var tempFile = TempFile.FromText(code))
            {
                var validationErrors = RunGlslValidator(stage, tempFile, targetOption);
                var spirv = RunGlslValidator(stage, tempFile, targetOption + " -H");
                var ast = RunGlslValidator(stage, tempFile, "-i");

                var outputs = new List<ShaderCompilerOutput>();
                outputs.Add(new ShaderCompilerOutput("Validation", null, validationErrors ?? "<No validation errors>"));

                if (target != ValidationOnly)
                {
                    outputs.Add(new ShaderCompilerOutput("SPIR-V", "SPIRV", spirv));
                }

                outputs.Add(new ShaderCompilerOutput("AST", null, ast));

                return new ShaderCompilerResult(null, outputs.ToArray());
            }
        }

        private static string RunGlslValidator(string stage, string codeFilePath, string arguments)
        {
            ProcessHelper.Run(
                Path.Combine(AppContext.BaseDirectory, "Binaries", "Glslang", "glslangValidator.exe"),
                $"-S {stage} -d {arguments} {codeFilePath}",
                out var stdOutput,
                out var stdError);

            if (!string.IsNullOrEmpty(stdError))
            {
                return stdError;
            }

            if (!string.IsNullOrEmpty(stdOutput))
            {
                return stdOutput;
            }

            return null;
        }
    }
}
