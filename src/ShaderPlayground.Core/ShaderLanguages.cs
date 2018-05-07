using ShaderPlayground.Core.Languages;

namespace ShaderPlayground.Core
{
    public static class ShaderLanguages
    {
        public static readonly IShaderLanguage[] All =
        {
            new HlslLanguage(),
            new GlslLanguage()
        };
    }
}
