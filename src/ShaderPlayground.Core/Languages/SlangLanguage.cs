namespace ShaderPlayground.Core.Languages
{
    internal sealed class SlangLanguage : IShaderLanguage
    {
        public string Name { get; } = LanguageNames.Slang;

        public string DefaultCode { get; } = DefaultSlangCode;

        public string FileExtension { get; } = "slang";

        private static readonly string DefaultSlangCode = @"RWStructuredBuffer<float> outputBuffer;

struct GenStruct<T>
{
	T x;
};

T test<T>(T val)
{
	return val;
}


[numthreads(4, 1, 1)]
void computeMain(uint3 dispatchThreadID : SV_DispatchThreadID)
{
	uint tid = dispatchThreadID.x;

	float inVal = float(tid);

	float outVal = test<float>(inVal);

	outputBuffer[tid] = outVal;
}";
    }
}
