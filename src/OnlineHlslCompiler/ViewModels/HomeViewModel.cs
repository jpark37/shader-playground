namespace OnlineHlslCompiler.ViewModels
{
    public class HomeViewModel
    {
        public string Code { get; set; }

        public Compiler Compiler { get; set; }
        public TargetProfile TargetProfile { get; set; }
        public string EntryPointName { get; set; }
    }
}