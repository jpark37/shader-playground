namespace OnlineHlslCompiler.ViewModels
{
    public class CompilationResultViewModel
    {
        public bool HasErrors { get; set; }
        public string Message { get; set; }
        public string Disassembly { get; set; }
    }
}