namespace KDR;

public interface IScanner
{
    public void Scan<TShader>(int primUpperBound, Scanline[] scanlines, Primitive<Vertex> viewTriangle, RenderDetails<TShader> renderDetails) where TShader : struct, IShader;
}
