namespace KDR;

using System.Numerics;

public class LinearScanner : IScanner
{
    IProjector Projector;

    public LinearScanner(IProjector projector) => Projector = projector;

    public void Scan<TShader>(int primUpperBound, Scanline[] scanlines, Primitive<Vertex> viewTriangle, RenderDetails<TShader> renderDetails) where TShader : struct, IShader
    {
        Vector3 viewAB = viewTriangle.V2.Position - viewTriangle.V1.Position;
        Vector3 viewAC = viewTriangle.V3.Position - viewTriangle.V1.Position;

        Vector3 unitNormal = Vector3.Normalize(Vector3.Cross(viewAB, viewAC));

        // No near/far plane clipping
        Primitive<Vector2> screenTriangle = new(
            Projector.Project(viewTriangle.V1.Position, renderDetails.Target.Midpoint),
            Projector.Project(viewTriangle.V2.Position, renderDetails.Target.Midpoint),
            Projector.Project(viewTriangle.V3.Position, renderDetails.Target.Midpoint)
        );

        Basis2 inverseTransform = new Basis2(
            screenTriangle.V2 - screenTriangle.V1,
            screenTriangle.V3 - screenTriangle.V1
        ).Inversed();

        Basis2 textureTransform = new Basis2(
            viewTriangle.V2.TexCoord - viewTriangle.V1.TexCoord,
            viewTriangle.V3.TexCoord - viewTriangle.V1.TexCoord
        ) * inverseTransform;

        Vector2 depthTransform = new Vector2(
            viewAB.Z,
            viewAC.Z
        ) * inverseTransform;

        Parallel.For(primUpperBound, primUpperBound + scanlines.Length, (y) => {
            Scanline currentScan = scanlines[y - primUpperBound];
            int offset = y * renderDetails.Target.Width;

            Vector2 pixelCenter = new Vector2(currentScan.LeftBound - 1, y) + new Vector2(0.5f, 0.5f) - screenTriangle.V1;

            Vector2 fragmentTexCoord = viewTriangle.V1.TexCoord + textureTransform * pixelCenter;
            float fragmentDepth = viewTriangle.V1.Position.Z + Vector2.Dot(depthTransform, pixelCenter);
            
            for (int x = currentScan.LeftBound; x < currentScan.RightBound; x++)
            {
                fragmentTexCoord += textureTransform.I;
                fragmentDepth += depthTransform.X;

                ShaderParam fragment = new ShaderParam(
                    x, y,
                    fragmentDepth,
                    fragmentTexCoord,
                    unitNormal
                );

                Rasterizer.Fill(offset + x, fragment, renderDetails);
            }
        });
    }
}
