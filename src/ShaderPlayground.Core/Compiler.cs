using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderPlayground.Core
{
    public static class Compiler
    {
        public static ShaderCompilerResult Compile(
            string code, 
            string languageName, 
            string compilerName, 
            Dictionary<string, string> arguments)
        {
            if (code.Length > 1000000)
            {
                throw new InvalidOperationException("Code exceeded maximum length.");
            }

            var language = ShaderLanguages.All.First(x => x.Name == languageName);
            var processor = language.Compilers.First(x => x.Name == compilerName);

            return processor.Compile(code, arguments);
        }
    }
}
