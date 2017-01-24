namespace OnlineHlslCompiler.Framework
{
    public class ShaderCompilationResult
    {
        public bool HasErrors { get; }
        public string Message { get; }
        public string Disassembly { get; }

        public ShaderCompilationResult(bool hasErrors, string message, string disassembly)
        {
            HasErrors = hasErrors;
            Message = message;
            Disassembly = disassembly;
        }
    }
}