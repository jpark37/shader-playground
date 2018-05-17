namespace ShaderPlayground.Core
{
    internal static class CommonParameters
    {
        private static readonly string[] ShaderStageOptions =
        {
            "vert",
            "tesc",
            "tese",
            "geom",
            "frag",
            "comp"
        };

        public static readonly ShaderCompilerParameter GlslShaderStage = new ShaderCompilerParameter(
            "ShaderStage", 
            "Shader stage", 
            ShaderCompilerParameterType.ComboBox, 
            ShaderStageOptions, 
            defaultValue: "frag");

        public static readonly ShaderCompilerParameter HlslEntryPoint = new ShaderCompilerParameter(
            "EntryPoint",
            "Entry point",
            ShaderCompilerParameterType.TextBox,
            defaultValue: "PSMain");

        public static readonly ShaderCompilerParameter SpirVEntryPoint = new ShaderCompilerParameter(
            "EntryPoint",
            "Entry point",
            ShaderCompilerParameterType.TextBox,
            defaultValue: "main",
            description: "Ignored when input language is GLSL.");

        public const string OutputLanguageParameterName = "OutputLanguage";

        public static ShaderCompilerParameter CreateOutputParameter(string[] languages)
        {
            return new ShaderCompilerParameter(
                OutputLanguageParameterName,
                "Output language",
                ShaderCompilerParameterType.ComboBox,
                languages,
                languages[0]);
        }
    }
}
