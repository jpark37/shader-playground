using OnlineShaderCompiler.Framework.Processors.Glslang;

namespace OnlineShaderCompiler.Framework.Languages
{
    public sealed class GlslLanguage : IShaderLanguage
    {
        public string Name { get; } = "GLSL";

        public string DefaultCode { get; } = DefaultGlslCode;

        public ShaderProcessorParameter[] LanguageParameters { get; } = new[]
        {
            new ShaderProcessorParameter("ShaderStage", "Shader stage", ShaderProcessorParameterType.ComboBox, ShaderStageOptions, defaultValue: "frag")
        };

        private static readonly string[] ShaderStageOptions =
        {
            "vert",
            "tesc",
            "tese",
            "geom",
            "frag",
            "comp"
        };

        public IShaderProcessor[] Processors { get; } = new IShaderProcessor[]
        {
            new GlslangGlslProcessor()
        };

        private static readonly string DefaultGlslCode = @"void main()
{
	gl_FragColor = vec4(0.4,0.4,0.8,1.0);
}";
    }
}
