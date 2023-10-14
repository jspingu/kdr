using System.Numerics;
using static ShaderUtil;

public class Material<TShader> where TShader : struct, IShader
{
    public TShader Shader;
    public Material(TShader shader) => Shader = shader;
}

public interface IShader
{
    int Compute(ShaderParam fragment);
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

public struct SimpleLight : IShader
{
    public int Compute(ShaderParam fragment) => Color(new Vector3(-fragment.Normal.Z * 0.5f + 0.5f) * 255);
}

public struct White : IShader
{
    public int Compute(ShaderParam fragment) => 0xFFFFFF;
}

public struct TextureMap : IShader
{
    public IntPtr Texture;
    
    public TextureMap(IntPtr texture) => Texture = texture;
    // public int Compute(ShaderParam fragment) => Color(VectorColor(NearestTexel(fragment.TexCoord, Texture)) * (-fragment.Normal.Z * 0.5f + 0.5f));
    public int Compute(ShaderParam fragment) => NearestTexel(fragment.TexCoord, Texture);
}

public struct DepthVisual : IShader
{
    public int Compute(ShaderParam fragment) => Color(new Vector3(fragment.Depth * 0.001f * 255));
}