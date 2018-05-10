using System;
using System.Collections.Generic;
using System.IO;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Dxc
{
    internal sealed class DxcCompiler : IShaderCompiler
    {
        public string Name { get; } = "dxc";
        public string DisplayName { get; } = "Microsoft DXC";
        public string Description { get; } = "New open-source HLSL-to-DXIL compiler (dxc.exe)";

        public ShaderCompilerParameter[] Parameters { get; } = new[]
        {
            new ShaderCompilerParameter("TargetProfile", "Target profile", ShaderCompilerParameterType.ComboBox, TargetProfileOptions, "vs_6_0"),
            new ShaderCompilerParameter("OutputFormat", "Output format", ShaderCompilerParameterType.ComboBox, OutputFormatOptions, "DXIL")
        };

        private static readonly string[] TargetProfileOptions =
        {
            "cs_6_0",
            "ds_6_0",
            "gs_6_0",
            "hs_6_0",
            "ps_6_0",
            "vs_6_0"
        };

        private static readonly string[] OutputFormatOptions =
        {
            "DXIL",
            "SPIR-V"
        };

        public ShaderCompilerResult Compile(string code, Dictionary<string, string> arguments)
        {
            var entryPoint = Validate.Identifier(arguments, "EntryPoint");
            var targetProfile = Validate.Option(arguments, "TargetProfile", TargetProfileOptions);
            var outputFormat = Validate.Option(arguments, "OutputFormat", OutputFormatOptions);

            using (var tempFile = TempFile.FromText(code))
            {
                var spirv = (outputFormat == "SPIR-V") ? "-spirv" : string.Empty;

                ProcessHelper.Run(
                    Path.Combine(AppContext.BaseDirectory, "Binaries", "Dxc", "dxc.exe"), 
                    $"{spirv} -T {targetProfile} -E {entryPoint} \"{tempFile.FilePath}\"",
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
                    new ShaderCompilerOutput("Disassembly", "DXIL", disassembly),
                    new ShaderCompilerOutput("Build output", null, stdError));
            }
        }
    }
}