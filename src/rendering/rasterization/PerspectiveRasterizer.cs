using static System.MathF;
using System.Numerics;

public class PerspectiveRasterizer : Rasterizer
{
    float TanHalfFOV;
    Vector2 ScreenToProjectionPlane;

    public PerspectiveRasterizer(int width, int height, float near, float far, float fieldOfView) : base(width, height, near, far)
    {
        TanHalfFOV = Tan(fieldOfView / 2f);
        ScreenToProjectionPlane = TanHalfFOV / Midpoint.X * new Vector2(1, -1);
    }

    public override Vector2 Project(Vector3 point) => Midpoint + Midpoint.X * new Vector2(point.X, -point.Y) / (point.Z * TanHalfFOV);

    public override void Scan<TShader>(int upperBound, int lowerBound, Scanline[] scanlines, Primitive<Vertex> viewTriangle, Canvas renderTarget, TShader shader)
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

        Parallel.For(upperBound, lowerBound, (y) => {
            Scanline currentScan = scanlines[y - upperBound];
            int offset = y * Width;
            
            for (int x = currentScan.LeftBound; x < currentScan.RightBound; x++)
            {
                Vector2 projPlane = (new Vector2(x, y) + new Vector2(0.5f, 0.5f) - Midpoint) * ScreenToProjectionPlane;

                float fragmentDepth = normalDisplacement / Vector3.Dot(new Vector3(projPlane, 1), normal);

                if (fragmentDepth > renderTarget.DepthBuffer[offset + x]) continue;
                renderTarget.DepthBuffer[offset + x] = fragmentDepth;

                Vector2 fragmentTexCoord = viewTriangle.V1.TexCoord + (textureTransform * (new Vector3(projPlane, 1) * fragmentDepth - viewTriangle.V1.Position)).ToVector2();

                ShaderParam fragment = new ShaderParam(
                    renderTarget.FrameBuffer[offset + x],
                    x, y,
                    fragmentDepth,
                    fragmentTexCoord,
                    unitNormal
                );

                renderTarget.FrameBuffer[offset + x] = shader.Compute(fragment);
            }
        });
    }
}