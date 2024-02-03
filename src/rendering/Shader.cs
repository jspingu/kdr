namespace KDR;

using System.Numerics;
using static ShaderUtil;

public abstract class Material
{
    internal abstract void CallTriangleDraw(Rasterizer rasterizer, Primitive<Vector2> screenTriangle, Primitive<Vertex> viewTriangle, Canvas renderTarget);
}

public class Material<TShader> : Material where TShader : struct, IShader
{
    public TShader Shader;
    public Material(TShader shader) => Shader = shader;

    internal override void CallTriangleDraw(Rasterizer rasterizer, Primitive<Vector2> screenTriangle, Primitive<Vertex> viewTriangle, Canvas renderTarget)
    {
        rasterizer.DrawTriangle(screenTriangle, viewTriangle, renderTarget, Shader);
    }
}

public interface IShader
{
    uint Compute(ShaderParam fragment);
}

public struct ShaderParam
{
    public uint Color;
    public int X, Y;
    public Vector2 TexCoord;
    public Vector3 Normal;
    public float Depth;

    public ShaderParam(uint color, int x, int y, float depth, Vector2 texCoord, Vector3 normal)
    {
        Color = color;

        X = x;
        Y = y;
        
        Depth = depth;
        TexCoord = texCoord;
        Normal = normal;
    }
}

public struct SimpleLight : IShader
{
    public uint Compute(ShaderParam fragment) => AlphaBlend(0xFFFFFF | (uint)((-fragment.Normal.Z * 0.5f + 0.5f) * 255) << 24, 0);
}

public struct White : IShader
{
    public uint Compute(ShaderParam fragment) => 0xFFFFFF;
}

public struct TextureMap : IShader
{
    public IntPtr Texture;
    
    public TextureMap(IntPtr texture) => Texture = texture;
    public uint Compute(ShaderParam fragment) 
    {
        uint alpha = (uint)((-fragment.Normal.Z * 0.5f + 0.5f) * 255);
        return AlphaBlend(NearestTexel(fragment.TexCoord, Texture) | alpha << 24, 0);
    }
}

public struct TestTransparency : IShader
{
    public uint Compute(ShaderParam fragment)
    {
        return AlphaBlend(0xAAFF0000, fragment.Color);
    }
}
