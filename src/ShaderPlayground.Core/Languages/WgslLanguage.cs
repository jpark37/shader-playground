namespace ShaderPlayground.Core.Languages
{
    internal sealed class WgslLanguage : IShaderLanguage
    {
        public string Name { get; } = LanguageNames.Wgsl;

        public string DefaultCode { get; } = DefaultWgslCode;

        public string FileExtension { get; } = "wgsl";

        private static readonly string DefaultWgslCode = @"# Vertex shader
const pos : array<vec2<f32>, 3> = array<vec2<f32>, 3>(
    vec2<f32>( 0.0,  0.5),
    vec2<f32>(-0.5, -0.5),
    vec2<f32>( 0.5, -0.5)
);

[[builtin(position)]]   var<out> Position    : vec4<f32>;
[[builtin(vertex_idx)]] var<in>  VertexIndex : i32;

[[stage(vertex)]]
fn vtx_main() -> void {
    Position = vec4<f32>(pos[VertexIndex], 0.0, 1.0);
    return;
}

# Fragment shader
[[location(0)]] var<out> outColor : vec4<f32>;

[[stage(fragment)]]
fn frag_main() -> void {
    outColor = vec4<f32>(1.0, 0.0, 0.0, 1.0);
    return;
}
";
    }
}
