using SharpDX.D3DCompiler;

namespace OnlineHlslCompiler.Framework.Fxc
{
    public class FxcCompiler : IShaderCompiler
    {
        static FxcCompiler()
        {
            // Preload native DLL, so that we can explicitly
            // load either 32-bit or 64-bit DLL.
            NativeMethods.LoadDll("d3dcompiler_47.dll");
        }

        public ShaderCompilationResult Compile(string code, string entryPoint, string targetProfile)
        {
            var compilationResult = ShaderBytecode.Compile(
                    code, entryPoint, targetProfile);

            string disassembly = null;
            if (!compilationResult.HasErrors && compilationResult.Bytecode != null)
                disassembly = compilationResult.Bytecode.Disassemble(DisassemblyFlags.None);

            var result = new ShaderCompilationResult(
                compilationResult.HasErrors || compilationResult.Bytecode == null,
                compilationResult.Message,
                disassembly);

            return result;
        }
    }
}