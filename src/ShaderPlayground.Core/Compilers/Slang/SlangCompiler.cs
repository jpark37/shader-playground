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
            CommonParameters.HlslEntryPoint,
            new ShaderCompilerParameter("Profile", "Profile", ShaderCompilerParameterType.ComboBox, ProfileOptions, "cs_5_0"),
            CommonParameters.CreateOutputParameter(new[] { LanguageNames.Hlsl, LanguageNames.Glsl })
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
