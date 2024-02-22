namespace KDR;

using static ShaderUtil;

public struct TextureMap : IShader
{
    Texture Texture;
    
    public TextureMap(Texture texture) => Texture = texture;

    public uint Compute(ShaderParam fragment) 
    {
        uint color = NearestTexel(fragment.TexCoord, Texture);
        uint darken = (uint)((-fragment.Normal.Z * 0.5f + 0.5f) * 255);

        return AlphaBlend(color & 0xFFFFFF | darken << 24, 0) | color & 0xFF000000;
    }
}
