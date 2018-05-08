using System;
using System.Collections.Generic;
using System.IO;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Fxc
{
    public sealed class FxcCompiler : IShaderCompiler
    {
        public string Name { get; } = "FXC";
        public string DisplayName { get; } = "Microsoft FXC";
        public string Description { get; } = "Legacy HLSL-to-DXBC compiler (fxc.exe)";

        public ShaderCompilerParameter[] Parameters { get; } = new[]
        {
            new ShaderCompilerParameter("TargetProfile", "Target profile", ShaderCompilerParameterType.ComboBox, TargetProfileOptions, "vs_5_0"),
            new ShaderCompilerParameter("DisableOptimizations", "Disable optimizations", ShaderCompilerParameterType.CheckBox),
            new ShaderCompilerParameter("OptimizationLevel", "Optimization level", ShaderCompilerParameterType.ComboBox, OptimizationLevelOptions, "1")
        };

        private static readonly string[] TargetProfileOptions =
        {
            "cs_4_0",
            "cs_4_1",
            "cs_5_0",
            "cs_5_1",
            "ds_5_0",
            "ds_5_1",
            "gs_4_0",
            "gs_4_1",
            "gs_5_0",
            "gs_5_1",
            "hs_5_0",
            "hs_5_1",
            "ps_2_0",
            "ps_2_a",
            "ps_2_b",
            "ps_2_sw",
            "ps_3_0",
            "ps_3_sw",
            "ps_4_0",
            "ps_4_1",
            "ps_5_0",
            "ps_5_1",
            "tx_1_0",
            "vs_1_1",
            "vs_2_0",
            "vs_2_a",
            "vs_2_sw",
            "vs_3_0",
            "vs_3_sw",
            "vs_4_0",
            "vs_4_1",
            "vs_5_0",
            "vs_5_1"
        };

        private static readonly string[] OptimizationLevelOptions =
        {
            "0",
            "1",
            "2",
            "3"
        };

        public ShaderCompilerResult Compile(string code, Dictionary<string, string> arguments)
        {
            var entryPoint = arguments["EntryPoint"];
            var targetProfile = arguments["TargetProfile"];
            var disableOptimizations = Convert.ToBoolean(arguments["DisableOptimizations"]);
            var optimizationLevel = Convert.ToInt32(arguments["OptimizationLevel"]);

            using (var tempFile = TempFile.FromText(code))
            {
                var args = $"--target {targetProfile} --entrypoint {entryPoint} --optimizationlevel {optimizationLevel}";

                if (disableOptimizations)
                {
                    args += " --disableoptimizations";
                }

                ProcessHelper.Run(
                    Path.Combine(AppContext.BaseDirectory, "Binaries", "Fxc", "ShaderPlayground.Shims.Fxc.exe"),
                    $"{args} \"{tempFile.FilePath}\"",
                    out var stdOutput,
                    out var stdError);

                int? selectedOutputIndex = null;

                var disassembly = stdOutput;
                if (string.IsNullOrWhiteSpace(stdOutput))
                {
                    disassembly = "Compilation error occurred; no disassembly available.";
                    selectedOutputIndex = 1;
                }

                return new ShaderCompilerResult(
                    selectedOutputIndex,
                    new ShaderCompilerOutput("Disassembly", "DXBC", disassembly),
                    new ShaderCompilerOutput("Build output", null, stdError));
            }
        }
    }
}
