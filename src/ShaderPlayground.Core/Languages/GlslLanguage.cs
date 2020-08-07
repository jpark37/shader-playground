namespace ShaderPlayground.Core.Languages
{
    internal sealed class GlslLanguage : IShaderLanguage
    {
        public string Name { get; } = LanguageNames.Glsl;

        public string DefaultCode { get; } = DefaultGlslCode;

        public string FileExtension { get; } = "glsl";

        private static readonly string DefaultGlslCode = @"#version 460

layout (location = 0) out vec4 fragColor;

void main()
{
	fragColor = vec4(0.4, 0.4, 0.8, 1.0);
}";
    }
}
