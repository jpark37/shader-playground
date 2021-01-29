using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Slang
{
    internal sealed class SlangCompiler : IShaderCompiler
    {
        public string Name { get; } = CompilerNames.Slang;
        public string DisplayName { get; } = "Slang";
        public string Url { get; } = "https://github.com/shader-slang/slang";
        public string Description { get; } = "Slang is a shading language that extends HLSL with new capabilities for building modular, extensible, and high-performance real-time shading systems";

        public string[] InputLanguages { get; } = { LanguageNames.Slang, LanguageNames.Hlsl };

        public ShaderCompilerParameter[] Parameters { get; } =
        {
            CommonParameters.CreateVersionParameter("slang"),
            // The default HLSL entry point (CommonParameters.HlslEntryPoint) uses PSMain, but the Slang sample is a compute shader with "computeMain"
            new ShaderCompilerParameter("EntryPoint", "Entry point", ShaderCompilerParameterType.TextBox, defaultValue: "computeMain"),
            new ShaderCompilerParameter("Profile", "Profile", ShaderCompilerParameterType.ComboBox, ProfileOptions, "cs_5_0"),
            new ShaderCompilerParameter("OptimizationLevel", "Optimization level", ShaderCompilerParameterType.ComboBox, OptimizationLevelOptions, "Default"),
            CommonParameters.CreateOutputParameter(new[] { LanguageNames.Dxil, LanguageNames.SpirV, LanguageNames.Dxbc, LanguageNames.Hlsl, LanguageNames.Glsl, LanguageNames.Cpp, LanguageNames.Cuda, LanguageNames.Ptx })
        };

        private static readonly string[] OptimizationLevelOptions =
        {
            "None",
            "Default",
            "High",
            "Maximal",
        };

        private static readonly string[] ProfileOptions =
        {
            "cs_4_0",
            "cs_4_1",
            "cs_5_0",
            "ds_5_0",
            "gs_4_0",
            "gs_4_1",
            "gs_5_0",
            "hs_5_0",
            "ps_4_0",
            "ps_4_1",
            "ps_5_0",
            "vs_4_0",
            "vs_4_1",
            "vs_5_0",
            "glsl_vertex",
            "glsl_tess_control",
            "glsl_tess_eval",
            "glsl_geometry",
            "glsl_fragment",
            "glsl_compute"
        };

        public ShaderCompilerResult Compile(ShaderCode shaderCode, ShaderCompilerArguments arguments)
        {
            var args = $"-entry {arguments.GetString("EntryPoint")}";
            args += $" -profile {arguments.GetString("Profile")}";

            var outputLanguage = arguments.GetString(CommonParameters.OutputLanguageParameterName);
            switch (outputLanguage)
            {
                case LanguageNames.Glsl:
                    args += " -target glsl";
                    break;

                case LanguageNames.Hlsl:
                    args += " -target hlsl";
                    break;
                    
                case LanguageNames.Cuda:
                    args += " -target cuda";
                    break;
                    
                case LanguageNames.Cpp:
                    args += " -target cpp";
                    break;

                case LanguageNames.Ptx:
                    args += " -target ptx";
                    break;

                case LanguageNames.Dxil:
                    args += " -target dxil-assembly";
                    break;

                case LanguageNames.SpirV:
                    args += " -target spirv-assembly";
                    break;

                case LanguageNames.Dxbc:
                    args += " -target dxbc-assembly";
                    break;
            }

            var optimizationLevel = arguments.GetString("OptimizationLevel");
            switch (optimizationLevel)
            {
                case "None":
                    args += " -O0";
                    break;
                case "High":
                    args += " -O2";
                    break;
                case "Maximal":
                    args += " -O3";
                    break;
                case "Default":
                    args += " -O";
                    break;
                default:
                    break;
            }

            using (var tempFile = TempFile.FromShaderCode(shaderCode))
            {
                var outputPath = $"{tempFile.FilePath}.out";

                ProcessHelper.Run(
                    CommonParameters.GetBinaryPath("slang", arguments, "slangc.exe"),
                    $"\"{tempFile.FilePath}\" -o \"{outputPath}\" {args}",
                    out var _,
                    out var stdError);

                var hasCompilationErrors = !string.IsNullOrWhiteSpace(stdError);

                var textOutput = FileHelper.ReadAllTextIfExists(outputPath);

                FileHelper.DeleteIfExists(outputPath);

                return new ShaderCompilerResult(
                    !hasCompilationErrors,
                    new ShaderCode(outputLanguage, textOutput),
                    hasCompilationErrors ? (int?)1 : null,
                    new ShaderCompilerOutput("Output", outputLanguage, textOutput),
                    new ShaderCompilerOutput("Errors", null, hasCompilationErrors ? stdError : "<No compilation errors>"));
            }
        }
    }
}
