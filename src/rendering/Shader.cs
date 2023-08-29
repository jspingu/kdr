using System.Numerics;
using static ShaderUtil;

public interface Shader
{
    int Compute(ShaderParam Fragment);
}

public struct ShaderParam
{
    public int x, y;
    public Vector2 TexCoord;
    public Vector3 BarycentricWeights, Normal;
    public float Depth;

    public ShaderParam(int x, int y, Vector3 BarycentricWeights, float Depth, Vector2 TexCoord, Vector3 Normal)
    {
        this.x = x;
        this.y = y;
        
        this.BarycentricWeights = BarycentricWeights;
        this.Depth = Depth;
        
        this.TexCoord = TexCoord;
        this.Normal = Normal;
    }
}

public class Hello : Shader
{
    public int Compute(ShaderParam Fragment)
    {
        Vector3 Rainbow = 255 * Fragment.BarycentricWeights;
        return Color((byte)Rainbow.X, (byte)Rainbow.Y, (byte)Rainbow.Z);
    }
}

public class SimpleLight : Shader
{
    public int Compute(ShaderParam Fragment) => Color(new Vector3(-Fragment.Normal.Z * 0.5f + 0.5f) * 255);
}

public class TextureMap : Shader
{
    public IntPtr Texture;
    
    public TextureMap(IntPtr Texture) => this.Texture = Texture;
    public int Compute(ShaderParam Fragment) => Color(NearestTexel(Fragment.TexCoord, Texture) * (-Fragment.Normal.Z * 0.5f + 0.5f));
}