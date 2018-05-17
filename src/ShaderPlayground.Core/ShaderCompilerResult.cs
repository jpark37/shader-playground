namespace ShaderPlayground.Core
{
    public sealed class ShaderCompilerResult
    {
        public ShaderCode PipeableOutput { get; }

        public int? SelectedOutputIndex { get; }
        public ShaderCompilerOutput[] Outputs { get; }

        internal ShaderCompilerResult(ShaderCode pipeableCode, int? selectedOutputIndex, params ShaderCompilerOutput[] outputs)
        {
            PipeableOutput = pipeableCode;
            SelectedOutputIndex = selectedOutputIndex;
            Outputs = outputs;
        }
    }
}
