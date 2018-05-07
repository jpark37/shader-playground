namespace ShaderPlayground.Core
{
    public interface IShaderLanguage
    {
        string Name { get; }
        string DefaultCode { get; }
        ShaderCompilerParameter[] LanguageParameters { get; }
        IShaderCompiler[] Compilers { get; }
    }
}
