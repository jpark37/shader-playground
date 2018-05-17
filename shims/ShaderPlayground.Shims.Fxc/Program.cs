using System;
using System.CommandLine;
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

        public static void Main(string[] args)
        {
            string targetProfile = null;
            string entryPoint = null;
            bool disableOptimizations = false;
            int optimizationLevel = 1;
            string objectFile = null;
            string assemblyFile = null;
            string errorsFile = null;
            string file = null;

            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("target", ref targetProfile, true, "Target profile");
                syntax.DefineOption("entrypoint", ref entryPoint, true, "Entry point");
                syntax.DefineOption("disableoptimizations", ref disableOptimizations, "Disable optimizations");
                syntax.DefineOption("optimizationlevel", ref optimizationLevel, "Optimization level");
                syntax.DefineOption("objectfile", ref objectFile, true, "Output object file");
                syntax.DefineOption("assemblyfile", ref assemblyFile, true, "Output assembly file");
                syntax.DefineOption("errorsfile", ref errorsFile, true, "Output errors and warnings file");
                syntax.DefineParameter("file", ref file, "File to compile");
            });

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
