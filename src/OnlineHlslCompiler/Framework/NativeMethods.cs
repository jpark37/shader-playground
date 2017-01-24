using System;
using System.Runtime.InteropServices;

namespace OnlineHlslCompiler.Framework
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);
    }
}