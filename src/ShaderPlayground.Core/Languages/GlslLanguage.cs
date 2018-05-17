namespace ShaderPlayground.Core.Languages
{
    internal sealed class GlslLanguage : IShaderLanguage
    {
        public string Name { get; } = LanguageNames.Glsl;

        public string DefaultCode { get; } = DefaultGlslCode;

        public string FileExtension { get; } = "glsl";

        private static readonly string DefaultGlslCode = @"void main()
{
	gl_FragColor = vec4(0.4, 0.4, 0.8, 1.0);
}";
    }
}
