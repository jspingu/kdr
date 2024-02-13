namespace KDR;

public struct RenderDetails<TShader> where TShader : struct, IShader
{
    public Canvas Target;
    public RasterizerFlags Options;
    public TShader Shader;

    public RenderDetails(Canvas target, RasterizerFlags options, TShader shader)
    {
        Target = target;
        Options = options;
        Shader = shader;
    }
}