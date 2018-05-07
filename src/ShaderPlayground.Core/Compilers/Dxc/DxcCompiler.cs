using System;
using System.Collections.Generic;

namespace ShaderPlayground.Core.Compilers.Dxc
{
    public sealed class DxcCompiler : IShaderCompiler
    {
        private static readonly IDxcLibrary Library = HlslDxcLib.CreateDxcLibrary();

        public string Name { get; } = "DXC";
        public string DisplayName { get; } = "Microsoft DXC";
        public string Description { get; } = "New open-source HLSL-to-DXIL compiler (dxc.exe)";

        public ShaderCompilerParameter[] Parameters { get; } = new[]
        {
            new ShaderCompilerParameter("TargetProfile", "Target profile", ShaderCompilerParameterType.ComboBox, TargetProfileOptions, "vs_6_0")
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

        public ShaderCompilerResult Compile(string code, Dictionary<string, string> arguments)
        {
            var dxcCompiler = HlslDxcLib.CreateDxcCompiler();

            var entryPoint = arguments["EntryPoint"];
            var targetProfile = arguments["TargetProfile"];

            var source = CreateBlobForText(code);
            var result = dxcCompiler.Compile(
                source, "hlsl.hlsl", 
                entryPoint, targetProfile, 
                new string[0], 0, 
                null, 0, 
                null);

            var errors = GetStringFromBlob(result.GetErrors());

            int? selectedOutputIndex = null;

            string disassembly;
            if (result.GetStatus() == 0)
            {
                var compiler = HlslDxcLib.CreateDxcCompiler();
                try
                {
                    var disassemblyBlob = compiler.Disassemble(result.GetResult());
                    disassembly = GetStringFromBlob(disassemblyBlob);
                }
                catch (Exception ex)
                {
                    disassembly = "Successfully compiled shader, but unable to disassemble compiled shader.\r\n" + ex.ToString();
                }
            }
            else
            {
                disassembly = "Compilation error occurred; no disassembly available.";
                selectedOutputIndex = 1;
            }

            return new ShaderCompilerResult(
                selectedOutputIndex,
                new ShaderCompilerOutput("Disassembly", "DXIL", disassembly),
                new ShaderCompilerOutput("Build output", null, errors));
        }

        private static IDxcBlobEncoding CreateBlobForText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            const uint CP_UTF16 = 1200;
            var source = Library.CreateBlobWithEncodingOnHeapCopy(text, (uint) (text.Length * 2), CP_UTF16);
            return source;
        }

        private static string GetStringFromBlob(IDxcBlob blob)
        {
            unsafe
            {
                blob = Library.GetBlobAstUf16(blob);
                return new string(blob.GetBufferPointer(), 0, (int) (blob.GetBufferSize() / 2));
            }
        }
    }
}