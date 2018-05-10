using ShaderPlayground.Core.Compilers.Glslang;
using ShaderPlayground.Core.Compilers.Mali;

namespace ShaderPlayground.Core.Languages
{
    internal sealed class GlslLanguage : IShaderLanguage
    {
        public string Name { get; } = "GLSL";

        public string DefaultCode { get; } = DefaultGlslCode;

        public ShaderCompilerParameter[] LanguageParameters { get; } = new[]
        {
            new ShaderCompilerParameter("ShaderStage", "Shader stage", ShaderCompilerParameterType.ComboBox, ShaderStageOptions, defaultValue: "frag")
        };

        public static readonly string[] ShaderStageOptions =
        {
            "vert",
            "tesc",
            "tese",
            "geom",
            "frag",
            "comp"
        };

        public IShaderCompiler[] Compilers { get; } = new IShaderCompiler[]
        {
            new GlslangGlslCompiler(),
            new MaliCompiler()
        };

        private static readonly string DefaultGlslCode = @"void main()
{
	gl_FragColor = vec4(0.4,0.4,0.8,1.0);
}";
    }
}
