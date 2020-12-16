using System.IO;
using SharpDX.D3DCompiler;

namespace ShaderPlayground.Shims.Fxc
{
    public static class Program
    {
        static Program()
        {
            // Preload native DLL, so that we can explicitly
            // load either 32-bit or 64-bit DLL.
            NativeMethods.LoadDll("d3dcompiler_47.dll");
        }

        /// <param name="targetProfile">Target profile</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="disableOptimizations">Disable optimizations</param>
        /// <param name="optimizationLevel">Optimization level</param>
        /// <param name="objectFile">Output object file</param>
        /// <param name="assemblyFile">Output assembly file</param>
        /// <param name="errorsFile">Output errors and warnings file</param>
        /// <param name="file">File to compile</param>
        public static void Main(string targetProfile, string entryPoint, bool disableOptimizations, int optimizationLevel, string objectFile, string assemblyFile, string errorsFile, string file)
        {
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

            var compilationResult = ShaderBytecode.CompileFromFile(
                file,
                entryPoint,
                targetProfile,
                shaderFlags);

            var hasCompilationErrors = compilationResult.HasErrors || compilationResult.Bytecode == null;

            File.WriteAllText(errorsFile, compilationResult.Message);

            if (!hasCompilationErrors)
            {
                File.WriteAllBytes(objectFile, compilationResult.Bytecode.Data);

                var disassembly = compilationResult.Bytecode.Disassemble(DisassemblyFlags.None);
                File.WriteAllText(assemblyFile, disassembly);
            }
        }
    }
}
