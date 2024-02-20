namespace KDR;

public struct TestTransparency : IShader
{
    public uint Compute(ShaderParam fragment) => 0xAAFF0000;
}
