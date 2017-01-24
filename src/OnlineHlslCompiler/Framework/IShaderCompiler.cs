namespace OnlineHlslCompiler.Framework
{
    public interface IShaderCompiler
    {
        ShaderCompilationResult Compile(string code, string entryPoint, string targetProfile);
    }
}