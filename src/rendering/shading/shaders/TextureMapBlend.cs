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
        uint color = NearestTexel(fragment.TexCoord, Texture);
        uint alpha = AlphaDivide((color >> 24) * Alpha); 

        return color & 0xFFFFFF | alpha << 24;
    }
}
