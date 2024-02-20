namespace KDR;

using static ShaderUtil;

public struct TextureMap : IShader
{
    Texture Texture;
    
    public TextureMap(Texture texture) => Texture = texture;
    public uint Compute(ShaderParam fragment) 
    {
        uint alpha = (uint)((-fragment.Normal.Z * 0.5f + 0.5f) * 255);
        return AlphaBlend(NearestTexel(fragment.TexCoord, Texture) | alpha << 24, 0);
    }
}
