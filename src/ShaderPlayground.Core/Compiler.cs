using System;
using System.Linq;
using ShaderPlayground.Core.Compilers.Dxc;
using ShaderPlayground.Core.Compilers.Fxc;
using ShaderPlayground.Core.Compilers.Glslang;
using ShaderPlayground.Core.Compilers.HlslCc;
using ShaderPlayground.Core.Compilers.Mali;
using ShaderPlayground.Core.Compilers.Slang;
using ShaderPlayground.Core.Compilers.SpirvCfg;
using ShaderPlayground.Core.Compilers.SpirVCross;
using ShaderPlayground.Core.Compilers.SpirVCrossIspc;
using ShaderPlayground.Core.Compilers.XShaderCompiler;
using ShaderPlayground.Core.Languages;

namespace ShaderPlayground.Core
{
    public static class Compiler
    {
        public static readonly IShaderLanguage[] AllLanguages =
        {
            new HlslLanguage(),
            new GlslLanguage(),
            new SlangLanguage()
        };

        public static readonly IShaderCompiler[] AllCompilers =
        {
            new DxcCompiler(),
            new FxcCompiler(),
            new GlslangCompiler(),
            new HlslCcCompiler(),
            new MaliCompiler(),
            new SlangCompiler(),
            new SpirVCrossCompiler(),
            new SpirVCrossIspcCompiler(),
            new SpirvCfgCompiler(),
            new XscCompiler()
        };

        public static ShaderCompilerResult Compile(
            ShaderCode shaderCode,
            params CompilationStep[] compilationSteps)
        {
            if ((shaderCode.Text != null && shaderCode.Text.Length > 1000000) ||
                (shaderCode.Binary != null && shaderCode.Binary.Length > 1000000))
            {
                throw new InvalidOperationException("Code exceeded maximum length.");
            }

            if (compilationSteps.Length == 0 || compilationSteps.Length > 5)
            {
                throw new InvalidOperationException("There must > 0 and <= 5 compilation steps.");
            }

            var eachShaderCode = shaderCode;
            ShaderCompilerResult result = null;

            foreach (var compilationStep in compilationSteps)
            {
                var compiler = AllCompilers.First(x => x.Name == compilationStep.CompilerName);

                if (!compiler.InputLanguages.Contains(eachShaderCode.Language))
                {
                    throw new InvalidOperationException($"Invalid input language '{eachShaderCode.Language}' for compiler '{compiler.DisplayName}'.");
                }

                var arguments = new ShaderCompilerArguments(
                    compiler,
                    compilationStep.Arguments);

                result = compiler.Compile(eachShaderCode, arguments);

                eachShaderCode = result.PipeableOutput;
            }

            return result;
        }
    }
}
