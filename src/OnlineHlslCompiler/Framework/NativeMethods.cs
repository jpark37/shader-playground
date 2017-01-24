using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;

namespace OnlineHlslCompiler.Framework
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        public static void LoadDll(string fileName)
        {
            var directory = HttpRuntime.BinDirectory;
            var libraryAddress = LoadLibrary(Environment.Is64BitProcess
                ? Path.Combine(directory, $"x64/{fileName}")
                : Path.Combine(directory, $"x86/{fileName}"));
            if (libraryAddress == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Could not load {fileName}: {errorCode}");
            }
        }
    }
}