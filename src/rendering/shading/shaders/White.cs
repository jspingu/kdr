namespace KDR;

public struct White : IShader
{
    public uint Compute(ShaderParam fragment) => 0xFFFFFF;
}
