namespace KDR;

using static ShaderUtil;

public struct TextureMapBlend : IShader
{
    Texture Texture;
    uint Alpha;

    public TextureMapBlend(Texture texture, uint alpha)
    {
        Texture = texture;
        Alpha = alpha;
    }

    public uint Compute(ShaderParam fragment)
    {
        return NearestTexel(fragment.TexCoord, Texture) & 0xFFFFFF | Alpha << 24;
    }
}
