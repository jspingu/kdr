namespace KDR;

using System.Numerics;

public class OrthographicRasterizer : Rasterizer
{
    public OrthographicRasterizer(int width, int height, float near, float far) : base(width, height, near, far) {}

    protected override Vector2 Project(Vector3 point) => Midpoint + new Vector2(point.X, -point.Y);

    protected override void Scan<TShader>(int upperBound, int lowerBound, Scanline[] scanlines, Primitive<Vertex> viewTriangle, Canvas renderTarget, TShader shader)
    {
        Basis2 inverseTransform = new Basis2(
            Project(viewTriangle.V2.Position) - Project(viewTriangle.V1.Position),
            Project(viewTriangle.V3.Position) - Project(viewTriangle.V1.Position)
        ).Inversed();

        Basis2 textureTransform = new Basis2(
            viewTriangle.V2.TexCoord - viewTriangle.V1.TexCoord,
            viewTriangle.V3.TexCoord - viewTriangle.V1.TexCoord
        ) * inverseTransform;

        Vector2 depthTransform = new Vector2(
            viewTriangle.V2.Position.Z - viewTriangle.V1.Position.Z,
            viewTriangle.V3.Position.Z - viewTriangle.V1.Position.Z
        ) * inverseTransform;

        Parallel.For(upperBound, lowerBound, (y) => {
            Scanline currentScan = scanlines[y - upperBound];
            int offset = y * Width;

            Vector2 pixelCenter = new Vector2(currentScan.LeftBound - 1, y) + new Vector2(0.5f, 0.5f) - Project(viewTriangle.V1.Position);

            Vector2 fragmentTexCoord = viewTriangle.V1.TexCoord + textureTransform * pixelCenter;
            float fragmentDepth = viewTriangle.V1.Position.Z + Vector2.Dot(depthTransform, pixelCenter);
            
            for (int x = currentScan.LeftBound; x < currentScan.RightBound; x++)
            {
                fragmentTexCoord += textureTransform.I;
                fragmentDepth += depthTransform.X;

                if (fragmentDepth > renderTarget.DepthBuffer[offset + x]) continue;
                renderTarget.DepthBuffer[offset + x] = fragmentDepth;

                ShaderParam fragment = new ShaderParam(
                    renderTarget.FrameBuffer[offset + x],
                    x, y,
                    fragmentDepth,
                    fragmentTexCoord,
                    Vector3.Zero
                );

                renderTarget.FrameBuffer[offset + x] = shader.Compute(fragment);
            }
        });
    }
}
