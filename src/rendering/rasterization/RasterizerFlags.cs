namespace KDR;

[Flags]
public enum RasterizerFlags
{
    None = 0,
    WriteDepth = 1,
    TestDepth = 2,
    CullBackFace = 4,
    AlphaBlend = 8,
    AlphaScissor = 16,
    All = ~0
}