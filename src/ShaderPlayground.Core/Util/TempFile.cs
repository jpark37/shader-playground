using System;
using System.IO;

namespace ShaderPlayground.Core.Util
{
    internal sealed class TempFile : IDisposable
    {
        public static TempFile FromShaderCode(ShaderCode shaderCode)
        {
            var result = new TempFile();

            switch (shaderCode.CodeType)
            {
                case ShaderCodeType.Text:
                    File.WriteAllText(result.FilePath, shaderCode.Text);
                    break;

                case ShaderCodeType.Binary:
                    File.WriteAllBytes(result.FilePath, shaderCode.Binary);
                    break;

                default:
                    throw new InvalidOperationException();
            }
            
            return result;
        }

        public string FilePath { get; }

        public TempFile()
        {
            FilePath = Path.GetTempFileName();
        }

        public void Dispose()
        {
            File.Delete(FilePath);
        }

        public static implicit operator string(TempFile tf) => tf.FilePath;
    }
}
