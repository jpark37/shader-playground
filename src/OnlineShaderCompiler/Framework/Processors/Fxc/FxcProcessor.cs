using System;
using System.Collections.Generic;
using SharpDX.D3DCompiler;

namespace OnlineShaderCompiler.Framework.Processors.Fxc
{
    public sealed class FxcProcessor : IShaderProcessor
    {
        static FxcProcessor()
        {
            // Preload native DLL, so that we can explicitly
            // load either 32-bit or 64-bit DLL.
            NativeMethods.LoadDll("d3dcompiler_47.dll");
        }

        public string Name { get; } = "FXC";
        public string DisplayName { get; } = "fxc.exe (old DXBC compiler)";

        public ShaderProcessorParameter[] Parameters { get; } = new[]
        {
            new ShaderProcessorParameter("TargetProfile", "Target profile", ShaderProcessorParameterType.ComboBox, TargetProfileOptions, "vs_5_0"),
            new ShaderProcessorParameter("DisableOptimizations", "Disable optimizations", ShaderProcessorParameterType.CheckBox),
            new ShaderProcessorParameter("OptimizationLevel", "Optimization level", ShaderProcessorParameterType.ComboBox, OptimizationLevelOptions, "1")
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

        public ShaderProcessorResult Process(string code, Dictionary<string, string> arguments)
        {
            var entryPoint = arguments["EntryPoint"];
            var targetProfile = arguments["TargetProfile"];
            var disableOptimizations = Convert.ToBoolean(arguments["DisableOptimizations"]);
            var optimizationLevel = Convert.ToInt32(arguments["OptimizationLevel"]);

            var shaderFlags = ShaderFlags.None;

            if (disableOptimizations)
            {
                shaderFlags |= ShaderFlags.SkipOptimization;
            }

            switch (optimizationLevel)
            {
                case 0:
                    shaderFlags |= ShaderFlags.OptimizationLevel0;
                    break;

                case 1:
                    shaderFlags |= ShaderFlags.OptimizationLevel1;
                    break;

                case 2:
                    shaderFlags |= ShaderFlags.OptimizationLevel2;
                    break;

                case 3:
                    shaderFlags |= ShaderFlags.OptimizationLevel3;
                    break;
            }

            var compilationResult = ShaderBytecode.Compile(
                code, 
                entryPoint, 
                targetProfile,
                shaderFlags);

            var disassembly = (!compilationResult.HasErrors && compilationResult.Bytecode != null)
                ? compilationResult.Bytecode.Disassemble(DisassemblyFlags.None)
                : "Compilation error occurred; no disassembly available.";

            return new ShaderProcessorResult(
                new ShaderProcessorOutput("Build output", null, compilationResult.Message ?? "<No build output>"),
                new ShaderProcessorOutput("Disassembly", "DXBC", disassembly));
        }
    }
}
