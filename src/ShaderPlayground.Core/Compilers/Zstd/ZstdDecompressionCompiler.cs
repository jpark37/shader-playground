using ShaderPlayground.Core.Util;

namespace ShaderPlayground.Core.Compilers.Zstd
{
    internal sealed class ZstdDecompressionCompiler : IShaderCompiler
    {
        public string Name { get; } = CompilerNames.ZstdDecompress;
        public string DisplayName { get; } = "Zstandard";
        public string Description { get; } = "Zstandard is a real-time compression algorithm, providing high compression ratios.";

        public string[] InputLanguages { get; } = { LanguageNames.Zstd };

        public ShaderCompilerParameter[] Parameters { get; } =
        {
            CommonParameters.CreateVersionParameter("zstd"),
            CommonParameters.CreateOutputParameter(new[] { LanguageNames.Dxbc, LanguageNames.Dxil, LanguageNames.Smolv, LanguageNames.SpirV })
        };

        public ShaderCompilerResult Compile(ShaderCode shaderCode, ShaderCompilerArguments arguments)
        {
            var outputLanguage = arguments.GetString(CommonParameters.OutputLanguageParameterName);

            using (var tempFile = TempFile.FromShaderCode(shaderCode))
            {
                var outputPath = $"{tempFile.FilePath}.out";

                ProcessHelper.Run(
                    CommonParameters.GetBinaryPath("zstd", arguments, "zstd.exe"),
                    $"-d -v \"{tempFile.FilePath}\" -o \"{outputPath}\"",
                    out var _,
                    out var stdError);

                var binaryOutput = FileHelper.ReadAllBytesIfExists(outputPath);

                FileHelper.DeleteIfExists(outputPath);

                return new ShaderCompilerResult(
                    true,
                    new ShaderCode(outputLanguage, binaryOutput),
                    null,
                    new ShaderCompilerOutput("Output", null, stdError));
            }
        }
    }
}
