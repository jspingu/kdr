using static System.MathF;
using System.Numerics;

public class PerspectiveRasterizer : Rasterizer
{
    float TanHalfFOV, ScreenToProjectionPlane;

    public PerspectiveRasterizer(int Width, int Height, float FieldOfView) : base(Width, Height)
    {
        TanHalfFOV = Tan(FieldOfView / 2f);
        ScreenToProjectionPlane = TanHalfFOV / Midpoint.X;
    }

    public override Vector2 Project(Vector3 Point) => Midpoint + Midpoint.X * Point.ToVector2() / (Point.Z * TanHalfFOV);

    public override void Scan<TShader>(int UpperBound, int LowerBound, Scanline[] Scanlines, SpatialPrimitive ViewTriangle, Canvas RenderTarget, TShader Shader)
    {
        Vector3 ViewAB = ViewTriangle.v2.Position - ViewTriangle.v1.Position;
        Vector3 ViewAC = ViewTriangle.v3.Position - ViewTriangle.v1.Position;

        Vector3 Normal = Vector3.Cross(ViewAB, ViewAC);

        Vector3 PerpAB = Vector3.Cross(Normal, ViewAB);
        Vector3 PerpAC = Vector3.Cross(ViewAC, Normal);

        Basis3 InverseTransform = new Basis3(
            new(PerpAC.X, PerpAB.X, 0),
            new(PerpAC.Y, PerpAB.Y, 0),
            new(PerpAC.Z, PerpAB.Z, 0)
        ) / Vector3.Dot(ViewAB, PerpAC);

        Basis3 TextureTransform = new Basis3(
            new(ViewTriangle.v2.TexCoord - ViewTriangle.v1.TexCoord, 0),
            new(ViewTriangle.v3.TexCoord - ViewTriangle.v1.TexCoord, 0),
            Vector3.Zero
        ) * InverseTransform;

        float NormalDisplacement = Vector3.Dot(ViewTriangle.v1.Position, Normal);

        Parallel.For(UpperBound, LowerBound, (y) => {
            Scanline CurrentScan = Scanlines[y - UpperBound];
            int Offset = y * Width;
            
            for (int x = CurrentScan.LeftBound; x < CurrentScan.RightBound; x++)
            {
                Vector2 ProjPlane = (new Vector2(x, y) + new Vector2(0.5f, 0.5f) - Midpoint) * ScreenToProjectionPlane;

                float FragmentDepth = NormalDisplacement / Vector3.Dot(new Vector3(ProjPlane, 1), Normal);

                if (FragmentDepth > RenderTarget.DepthBuffer[Offset + x]) continue;
                RenderTarget.DepthBuffer[Offset + x] = FragmentDepth;

                Vector2 FragmentTexCoord = ViewTriangle.v1.TexCoord + (TextureTransform * (new Vector3(ProjPlane, 1) * FragmentDepth - ViewTriangle.v1.Position)).ToVector2();

                ShaderParam Fragment = new ShaderParam(
                    x, y,
                    FragmentDepth,
                    FragmentTexCoord,
                    Normal
                );

                RenderTarget.FrameBuffer[Offset + x] = Shader.Compute(Fragment);
            }
        });
    }
}