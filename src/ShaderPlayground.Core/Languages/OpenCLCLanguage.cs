namespace ShaderPlayground.Core.Languages
{
    internal sealed class OpenCLCLanguage : IShaderLanguage
    {
        public string Name { get; } = LanguageNames.OpenCLC;

        public string DefaultCode { get; } = DefaultOpenCLCode;

        public string FileExtension { get; } = "cl";

        private static readonly string DefaultOpenCLCode = @"__kernel void foo(__global int4* data, __constant int4* c) {
  *data = *c;
}";
    }
}
