using System.ComponentModel.DataAnnotations;

namespace OnlineHlslCompiler.ViewModels
{
    public enum Compiler
    {
        [Display(Name = "New compiler (dxc.exe)")]
        NewCompiler,

        [Display(Name = "Old compiler (fxc.exe)")]
        OldCompiler
    }
}