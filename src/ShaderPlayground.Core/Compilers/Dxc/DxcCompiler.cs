using System;
using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Dxc
{
    internal sealed class DxcCompiler : IShaderCompiler
    {
        public string Name { get; } = CompilerNames.Dxc;
        public string DisplayName { get; } = "Microsoft DXC";
        public string Url { get; } = "https://github.com/Microsoft/DirectXShaderCompiler";
        public string Description { get; } = "New open-source HLSL-to-DXIL compiler (dxc.exe)";

        public string[] InputLanguages { get; } = { LanguageNames.Hlsl };

        public ShaderCompilerParameter[] Parameters { get; } = new[]
        {
            CommonParameters.CreateVersionParameter("dxc"),
            CommonParameters.HlslEntryPoint,
            new ShaderCompilerParameter("TargetProfile", "Target profile", ShaderCompilerParameterType.ComboBox, TargetProfileOptions, "ps_6_0"),
            new ShaderCompilerParameter("DisableOptimizations", "Disable optimizations", ShaderCompilerParameterType.CheckBox),
            new ShaderCompilerParameter("OptimizationLevel", "Optimization level", ShaderCompilerParameterType.ComboBox, OptimizationLevelOptions, "3"),
            CommonParameters.CreateOutputParameter(new[] { LanguageNames.Dxil, LanguageNames.SpirV }),
            new ShaderCompilerParameter("SpirvTarget", "SPIR-V target", ShaderCompilerParameterType.ComboBox, SpirvTargetOptions, "vulkan1.0", filter: new ParameterFilter(CommonParameters.OutputLanguageParameterName, LanguageNames.SpirV)),
        };

        private static readonly string[] TargetProfileOptions =
        {
            "cs_6_0",
            "cs_6_1",
            "cs_6_2",
            "cs_6_3",
            "cs_6_4",
            "ds_6_0",
            "ds_6_1",
            "ds_6_2",
            "ds_6_3",
            "ds_6_4",
            "gs_6_0",
            "gs_6_1",
            "gs_6_2",
            "gs_6_3",
            "gs_6_4",
            "hs_6_0",
            "hs_6_1",
            "hs_6_2",
            "hs_6_3",
            "hs_6_4",
            "ps_6_0",
            "ps_6_1",
            "ps_6_2",
            "ps_6_3",
            "ps_6_4",
            "vs_6_0",
            "vs_6_1",
            "vs_6_2",
            "vs_6_3",
            "vs_6_4"
        };

        private static readonly string[] OptimizationLevelOptions =
        {
            "0",
            "1",
            "2",
            "3",
            "4"
        };

        private static readonly string[] SpirvTargetOptions =
        {
            "vulkan1.0",
            "vulkan1.1"
        };
        
        public ShaderCompilerResult Compile(ShaderCode shaderCode, ShaderCompilerArguments arguments)
        {
            var entryPoint = arguments.GetString("EntryPoint");
            var targetProfile = arguments.GetString("TargetProfile");
            var disableOptimizations = arguments.GetBoolean("DisableOptimizations");
            var optimizationLevel = Convert.ToInt32(arguments.GetString("OptimizationLevel"));
            var outputLanguage = arguments.GetString(CommonParameters.OutputLanguageParameterName);

            var spirv = (outputLanguage == LanguageNames.SpirV) ? $"-spirv -fspv-target-env={arguments.GetString("SpirvTarget")}" : string.Empty;

            using (var tempFile = TempFile.FromShaderCode(shaderCode))
            {
                var fcPath = $"{tempFile.FilePath}.fc";
                var fePath = $"{tempFile.FilePath}.fe";
                var foPath = $"{tempFile.FilePath}.fo";

                var args = $"{spirv} -T {targetProfile} -E {entryPoint} -O{optimizationLevel} -Fc \"{fcPath}\" -Fe \"{fePath}\" -Fo \"{foPath}\"";

                if (disableOptimizations)
                {
                    args += " -Od";
                }

                args += $" \"{tempFile.FilePath}\"";

                var dxcPath = CommonParameters.GetBinaryPath("dxc", arguments, "dxc.exe");
                ProcessHelper.Run(
                    dxcPath, 
                    args,
                    out var _,
                    out var _);

                int? selectedOutputIndex = null;

                var disassembly = FileHelper.ReadAllTextIfExists(fcPath);
                if (string.IsNullOrWhiteSpace(disassembly))
                {
                    disassembly = "<Compilation error occurred>";
                    selectedOutputIndex = 2;
                }

                var binaryOutput = FileHelper.ReadAllBytesIfExists(foPath);
                var buildOutput = FileHelper.ReadAllTextIfExists(fePath);

                FileHelper.DeleteIfExists(fcPath);
                FileHelper.DeleteIfExists(fePath);
                FileHelper.DeleteIfExists(foPath);

                // Run again to get AST (can't be done in combination with output files, above).
                ProcessHelper.Run(
                    dxcPath,
                    $"-T {targetProfile} -E {entryPoint} -ast-dump \"{tempFile.FilePath}\"",
                    out var stdOutputAst,
                    out var _);

                return new ShaderCompilerResult(
                    selectedOutputIndex == null,
                    new ShaderCode(outputLanguage, binaryOutput),
                    selectedOutputIndex,
                    new ShaderCompilerOutput("Disassembly", outputLanguage, disassembly),
                    new ShaderCompilerOutput("AST", null, stdOutputAst),
                    new ShaderCompilerOutput("Build output", null, buildOutput));
            }
        }
    }
}