using System;

namespace OnlineHlslCompiler.Framework.Dxc
{
    public class DxcCompiler : IShaderCompiler
    {
        private static readonly IDxcLibrary Library = HlslDxcLib.CreateDxcLibrary();

        public ShaderCompilationResult Compile(string code, string entryPoint, string targetProfile)
        {
            var dxcCompiler = HlslDxcLib.CreateDxcCompiler();

            var source = CreateBlobForText(code);
            var result = dxcCompiler.Compile(
                source, "hlsl.hlsl", 
                entryPoint, targetProfile, 
                new string[0], 0, 
                null, 0, 
                null);

            if (result.GetStatus() == 0)
            {
                result.GetResult();

                var compiler = HlslDxcLib.CreateDxcCompiler();
                try
                {
                    var disassemblyBlob = compiler.Disassemble(result.GetResult());
                    string disassemblyText = GetStringFromBlob(disassemblyBlob);
                    return new ShaderCompilationResult(false, null, disassemblyText);
                }
                catch (Exception ex)
                {
                    return new ShaderCompilationResult(
                        true,
                        "Successfully compiled shader, but unable to disassemble compiled shader.\r\n" + ex.ToString(),
                        null);
                }
            }
            else
            {
                var errors = GetStringFromBlob(result.GetErrors());
                return new ShaderCompilationResult(true, errors, null);
            }
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