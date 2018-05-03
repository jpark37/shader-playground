using System;
using System.Collections.Generic;

namespace OnlineShaderCompiler.Framework.Processors.Glslang
{
    public sealed class GlslangGlslProcessor : IShaderProcessor
    {
        public string Name { get; } = "glslang";

        public ShaderProcessorParameter[] Parameters { get; } =
        {
            // TODO
        };

        public ShaderProcessorResult Process(string code, Dictionary<string, string> arguments)
        {
            throw new NotImplementedException();
        }
    }
}
