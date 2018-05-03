using System;
using System.Collections.Generic;

namespace OnlineShaderCompiler.Framework.Processors.Dxc
{
    public sealed class DxcProcessor : IShaderProcessor
    {
        private static readonly IDxcLibrary Library = HlslDxcLib.CreateDxcLibrary();

        public string Name { get; } = "dxc.exe (new DXIL compiler)";

        public ShaderProcessorParameter[] Parameters { get; } = new[]
        {
            new ShaderProcessorParameter("TargetProfile", "Target profile", ShaderProcessorParameterType.ComboBox, TargetProfileOptions, "vs_6_0")
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

        public ShaderProcessorResult Process(string code, Dictionary<string, string> arguments)
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
            }

            return new ShaderProcessorResult(
                new ShaderProcessorOutput("Build output", null, errors),
                new ShaderProcessorOutput("Disassembly", "DXIL", disassembly));
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