using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Mali
{
    internal sealed class MaliCompiler : IShaderCompiler
    {
        static MaliCompiler()
        {
            var coreOptions = new List<string>();

            foreach (var versionDirectory in Directory.GetDirectories(Path.Combine(AppContext.BaseDirectory, "Binaries", "mali")))
            {
                ProcessHelper.Run(
                    Path.Combine(versionDirectory, "malisc.exe"),
                    "--list",
                    out var stdOutput,
                    out var _);

                // Extract cores from output.
                var coreRegex = new Regex(@"\s+(Mali-[a-zA-Z0-9]+) <");
                var matches = coreRegex.Matches(stdOutput);

                coreOptions.AddRange(matches
                    .Cast<Match>()
                    .Select(x => x.Groups[1].Value));
            }

            CoreOptions = 
                coreOptions
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public string Name { get; } = CompilerNames.Mali;
        public string DisplayName { get; } = "Mali offline compiler";
        public string Url { get; } = "https://developer.arm.com/products/software-development-tools/graphics-development-tools/mali-offline-compiler";
        public string Description { get; } = "ARM Mali offline compiler";

        public string[] InputLanguages { get; } = { LanguageNames.Glsl, LanguageNames.SpirV };

        public ShaderCompilerParameter[] Parameters { get; } = new[]
        {
            CommonParameters.CreateVersionParameter("mali"),
            CommonParameters.GlslShaderStage,
            CommonParameters.SpirVEntryPoint,
            new ShaderCompilerParameter("Core", "Core", ShaderCompilerParameterType.ComboBox, CoreOptions, "Mali-G72")
        };

        private static readonly string[] CoreOptions;

        public ShaderCompilerResult Compile(ShaderCode shaderCode, ShaderCompilerArguments arguments)
        {
            var stage = GetStageFlag(arguments.GetString("ShaderStage"));
            var core = arguments.GetString("Core");

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
                    CommonParameters.GetBinaryPath("mali", arguments, "malisc.exe"),
                    $"{stage} -c {core} {args} {tempFile.FilePath}",
                    out var stdOutput,
                    out var stdError);

                return new ShaderCompilerResult(
                    true,
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
