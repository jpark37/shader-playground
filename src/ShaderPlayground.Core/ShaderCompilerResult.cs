namespace ShaderPlayground.Core
{
    public sealed class ShaderCompilerResult
    {
        public bool Success { get; }
        public ShaderCode PipeableOutput { get; }

        public int? SelectedOutputIndex { get; }
        public ShaderCompilerOutput[] Outputs { get; }

        internal ShaderCompilerResult(bool success, ShaderCode pipeableCode, int? selectedOutputIndex, params ShaderCompilerOutput[] outputs)
        {
            Success = success;
            PipeableOutput = pipeableCode;
            SelectedOutputIndex = selectedOutputIndex;
            Outputs = outputs;
        }
    }
}
