namespace KDR;

using System.Numerics;

public class PerspectiveScanner : IScanner
{
    float TanHalfFOV;

    public PerspectiveScanner(float fieldOfView) => TanHalfFOV = MathF.Tan(fieldOfView / 2f);

    public void Scan<TShader>(int primUpperBound, Scanline[] scanlines, Primitive<Vertex> viewTriangle, RenderDetails<TShader> renderDetails) where TShader : struct, IShader
    {
        Vector3 viewAB = viewTriangle.V2.Position - viewTriangle.V1.Position;
        Vector3 viewAC = viewTriangle.V3.Position - viewTriangle.V1.Position;

        Vector3 normal = Vector3.Cross(viewAB, viewAC);
        Vector3 unitNormal = Vector3.Normalize(normal);

        Vector3 perpAB = Vector3.Cross(normal, viewAB);
        Vector3 perpAC = Vector3.Cross(viewAC, normal);

        Basis3 inverseTransform = new Basis3(
            new(perpAC.X, perpAB.X, 0),
            new(perpAC.Y, perpAB.Y, 0),
            new(perpAC.Z, perpAB.Z, 0)
        ) / Vector3.Dot(viewAB, perpAC);

        Basis3 textureTransform = new Basis3(
            new(viewTriangle.V2.TexCoord - viewTriangle.V1.TexCoord, 0),
            new(viewTriangle.V3.TexCoord - viewTriangle.V1.TexCoord, 0),
            Vector3.Zero
        ) * inverseTransform;

        float normalDisplacement = Vector3.Dot(viewTriangle.V1.Position, normal);

        Vector2 ScreenToProjectionPlane = TanHalfFOV / renderDetails.Target.Midpoint.X * new Vector2(1, -1);

        Parallel.For(primUpperBound, primUpperBound + scanlines.Length, (y) => {
            Scanline currentScan = scanlines[y - primUpperBound];
            int offset = y * renderDetails.Target.Width;
            
            for (int x = currentScan.LeftBound; x < currentScan.RightBound; x++)
            {
                Vector2 projPlane = (new Vector2(x, y) + new Vector2(0.5f, 0.5f) - renderDetails.Target.Midpoint) * ScreenToProjectionPlane;

                float fragmentDepth = normalDisplacement / Vector3.Dot(new Vector3(projPlane, 1), normal);
                Vector2 fragmentTexCoord = viewTriangle.V1.TexCoord + (textureTransform * (new Vector3(projPlane, 1) * fragmentDepth - viewTriangle.V1.Position)).ToVector2();

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
