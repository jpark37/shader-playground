using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Glslang
{
    public sealed class GlslangGlslCompiler : IShaderCompiler
    {
        public string Name { get; } = "GLSLANG";
        public string DisplayName { get; } = "glslangvalidator";
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

            using (var tempFile = new TempFile())
            {
                File.WriteAllText(tempFile, code);

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
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "glslangvalidator.exe",
                Arguments = $"-S {stage} -d {arguments} {codeFilePath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = System.Diagnostics.Process.Start(processStartInfo))
            {
                process.WaitForExit(4000);

                var stdOut = process.StandardOutput.ReadToEnd();
                var stdError = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(stdError))
                {
                    return stdError;
                }

                if (!string.IsNullOrEmpty(stdOut))
                {
                    return stdOut;
                }

                return null;
            }
        }
    }
}
