namespace KDR;

using System.Numerics;

public abstract class Material
{
    internal abstract void CallTriangleDraw(Rasterizer rasterizer, Primitive<Vector2> screenTriangle, Primitive<Vertex> viewTriangle, Canvas renderTarget, RasterizerFlags options);
}

public class Material<TShader> : Material where TShader : struct, IShader
{
    public TShader Shader;
    public Material(TShader shader) => Shader = shader;

    internal override void CallTriangleDraw(Rasterizer rasterizer, Primitive<Vector2> screenTriangle, Primitive<Vertex> viewTriangle, Canvas renderTarget, RasterizerFlags options)
    {
        rasterizer.DrawTriangle(screenTriangle, viewTriangle, new RenderDetails<TShader>(renderTarget, options, Shader));
    }
}

public interface IShader
{
    uint Compute(ShaderParam fragment);
}

public struct ShaderParam
{
    public int X, Y;
    public Vector2 TexCoord;
    public Vector3 Normal;
    public float Depth;

    public ShaderParam(int x, int y, float depth, Vector2 texCoord, Vector3 normal)
    {
        X = x;
        Y = y;
        
        Depth = depth;
        TexCoord = texCoord;
        Normal = normal;
    }
}
