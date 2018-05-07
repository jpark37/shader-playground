namespace ShaderPlayground.Core
{
    public sealed class ShaderCompilerResult
    {
        public int? SelectedOutputIndex { get; }
        public ShaderCompilerOutput[] Outputs { get; }

        public ShaderCompilerResult(int? selectedOutputIndex, params ShaderCompilerOutput[] outputs)
        {
            SelectedOutputIndex = selectedOutputIndex;
            Outputs = outputs;
        }
    }
}
