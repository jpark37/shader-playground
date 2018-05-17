using System.IO;

namespace ShaderPlayground.Core.Util
{
    internal static class FileHelper
    {
        public static void DeleteIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static string ReadAllTextIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
        }

        public static byte[] ReadAllBytesIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            return null;
        }
    }
}
