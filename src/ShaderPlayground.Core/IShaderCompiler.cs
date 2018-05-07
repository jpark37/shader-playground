using System.Collections.Generic;

namespace ShaderPlayground.Core
{
    public interface IShaderCompiler
    {
        string Name { get; }
        string DisplayName { get; }
        string Description { get; }
        
        ShaderCompilerParameter[] Parameters { get; }

        ShaderCompilerResult Compile(string code, Dictionary<string, string> arguments);
    }
}
