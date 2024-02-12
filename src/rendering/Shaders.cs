using KDR;
using static KDR.ShaderUtil;

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
    IntPtr Texture;
    
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

public struct TextureMapBlend : IShader
{
    IntPtr Texture;
    uint Alpha;

    public TextureMapBlend(IntPtr texture, uint alpha)
    {
        Texture = texture;
        Alpha = alpha;
    }

    public uint Compute(ShaderParam fragment)
    {
        return AlphaBlend(NearestTexel(fragment.TexCoord, Texture) | Alpha << 24, fragment.Color);
    }
}
