namespace ShaderPlayground.Core
{
    public interface IShaderLanguage
    {
        string Name { get; }
        string DefaultCode { get; }
        string FileExtension { get; }
    }
}
