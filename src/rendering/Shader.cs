using System.Numerics;
using static ShaderUtil;

public class Material<TShader> where TShader : struct, IShader
{
    public TShader Shader;
    public Material(TShader Shader) => this.Shader = Shader;
}

public interface IShader
{
    int Compute(ShaderParam Fragment);
}

public struct ShaderParam
{
    public int x, y;
    public Vector2 TexCoord;
    public Vector3 Normal;
    public float Depth;

    public ShaderParam(int x, int y, float Depth, Vector2 TexCoord, Vector3 Normal)
    {
        this.x = x;
        this.y = y;
        
        this.Depth = Depth;
        this.TexCoord = TexCoord;
        this.Normal = Normal;
    }
}

public struct SimpleLight : IShader
{
    public int Compute(ShaderParam Fragment) => Color(new Vector3(-Fragment.Normal.Z * 0.5f + 0.5f) * 255);
}

public struct White : IShader
{
    public int Compute(ShaderParam Fragment) => 0xFFFFFF;
}

public struct TextureMap : IShader
{
    public IntPtr Texture;
    
    public TextureMap(IntPtr Texture) => this.Texture = Texture;
    // public int Compute(ShaderParam Fragment) => Color(VectorColor(NearestTexel(Fragment.TexCoord, Texture)) * (-Fragment.Normal.Z * 0.5f + 0.5f));
    public int Compute(ShaderParam Fragment) => NearestTexel(Fragment.TexCoord, Texture);
}

public struct DepthVisual : IShader
{
    public int Compute(ShaderParam Fragment) => Color(new Vector3(Fragment.Depth * 0.001f * 255));
}