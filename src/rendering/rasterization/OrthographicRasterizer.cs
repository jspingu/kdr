using System.Numerics;

public class OrthographicRasterizer : Rasterizer
{
    public OrthographicRasterizer(int Width, int Height) : base(Width, Height) {}

    public override Vector2 Project(Vector3 Point) => Midpoint + Point.ToVector2();

    public override void Scan<TShader>(int UpperBound, int LowerBound, Scanline[] Scanlines, SpatialPrimitive ViewTriangle, Canvas RenderTarget, TShader Shader)
    {
        Basis2 InverseTransform = new Basis2(
            (ViewTriangle.v2.Position - ViewTriangle.v1.Position).ToVector2(),
            (ViewTriangle.v3.Position - ViewTriangle.v1.Position).ToVector2()
        ).Inversed();

        Basis2 TextureTransform = new Basis2(
            ViewTriangle.v2.TexCoord - ViewTriangle.v1.TexCoord,
            ViewTriangle.v3.TexCoord - ViewTriangle.v1.TexCoord
        ) * InverseTransform;

        Vector2 DepthTransform = new Vector2(
            ViewTriangle.v2.Position.Z - ViewTriangle.v1.Position.Z,
            ViewTriangle.v3.Position.Z - ViewTriangle.v1.Position.Z
        ) * InverseTransform;

        Parallel.For(UpperBound, LowerBound, (y) => {
            Scanline CurrentScan = Scanlines[y - UpperBound];
            int Offset = y * Width;

            Vector2 PixelCenter = new Vector2(CurrentScan.LeftBound - 1, y) + new Vector2(0.5f, 0.5f) - Project(ViewTriangle.v1.Position);

            Vector2 FragmentTexCoord = ViewTriangle.v1.TexCoord + TextureTransform * PixelCenter;
            float FragmentDepth = ViewTriangle.v1.Position.Z + Vector2.Dot(DepthTransform, PixelCenter);
            
            for (int x = CurrentScan.LeftBound; x < CurrentScan.RightBound; x++)
            {
                FragmentTexCoord += TextureTransform.i;
                FragmentDepth += DepthTransform.X;

                if (FragmentDepth > RenderTarget.DepthBuffer[Offset + x]) continue;
                RenderTarget.DepthBuffer[Offset + x] = FragmentDepth;

                ShaderParam Fragment = new ShaderParam(
                    x, y,
                    FragmentDepth,
                    FragmentTexCoord,
                    Vector3.Zero
                );

                RenderTarget.FrameBuffer[Offset + x] = Shader.Compute(Fragment);
            }
        });
    }
}