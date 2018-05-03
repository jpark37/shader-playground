using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OnlineShaderCompiler.Framework
{
    public static class NativeMethods
    {
        public static string RootDirectory { get; internal set; }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        public static void LoadDll(string fileName)
        {
            var libraryAddress = LoadLibrary(Environment.Is64BitProcess
                ? Path.Combine(RootDirectory, $"Native/x64/{fileName}")
                : Path.Combine(RootDirectory, $"Native/x86/{fileName}"));
            if (libraryAddress == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Could not load {fileName}: {errorCode}");
            }
        }
    }
}