using System;
using System.IO;

namespace ShaderPlayground.Core.Util
{
    internal sealed class TempDirectory : IDisposable
    {
        public string DirectoryPath { get; }

        public TempDirectory()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(DirectoryPath);
        }

        public void Dispose()
        {
            Directory.Delete(DirectoryPath, true);
        }
    }
}
