namespace KDR;

using static KDR.ShaderUtil;

public struct SimpleLight : IShader
{
    public uint Compute(ShaderParam fragment) => AlphaBlend(0xFFFFFF | (uint)((-fragment.Normal.Z * 0.5f + 0.5f) * 255) << 24, 0);
}
