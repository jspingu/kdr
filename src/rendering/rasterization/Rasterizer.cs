using static System.MathF;
using static MathUtil;
using System.Numerics;

public abstract class Rasterizer
{
    public int Width, Height;
    public Vector2 Midpoint;

    public Rasterizer(int Width, int Height)
    {
        this.Width = Width;
        this.Height = Height;

        Midpoint = new Vector2(Width, Height) / 2f;
    }

    public void DrawModel<TShader>(Vector3[] ViewSpaceVertices, Vector2[] TextureVertices, IndexedFace[] Faces, Canvas RenderTarget, TShader Shader) where TShader : struct, IShader
    {
        Vector2[] ScreenSpaceVertices = new Vector2[ViewSpaceVertices.Length];

        for(int i = 0; i < ScreenSpaceVertices.Length; i++)
        {
            ScreenSpaceVertices[i] = Project(ViewSpaceVertices[i]);
        }

        foreach(IndexedFace Face in Faces)
        {
            Primitive ScreenTriangle = new(
                ScreenSpaceVertices[Face.v1],
                ScreenSpaceVertices[Face.v2],
                ScreenSpaceVertices[Face.v3]
            );

            SpatialPrimitive ViewTriangle = new(
                new Vertex(ViewSpaceVertices[Face.v1], TextureVertices[Face.t1]),
                new Vertex(ViewSpaceVertices[Face.v2], TextureVertices[Face.t2]),
                new Vertex(ViewSpaceVertices[Face.v3], TextureVertices[Face.t3])
            );

            DrawTriangle(ScreenTriangle, ViewTriangle, RenderTarget, Shader);
        }
    }

    public void DrawTriangle<TShader>(Primitive ScreenTriangle, SpatialPrimitive ViewTriangle, Canvas RenderTarget, TShader Shader) where TShader : struct, IShader
    {
        Vector2 AToB = ScreenTriangle.b - ScreenTriangle.a;
        Vector2 AToC = ScreenTriangle.c - ScreenTriangle.a;

        float AToBFirst = Vector2.Dot(AToB.ClockwiseNormal(), AToC);

        if (AToBFirst <= 0) return; // Vertices not in clockwise order or triangle sides are parallel

        float PrimMinY = ScreenTriangle.a.Y + Min(Min(AToB.Y, 0), AToC.Y);
        float PrimMaxY = ScreenTriangle.a.Y + Max(Max(AToB.Y, 0), AToC.Y);

        int PrimUpperBound = Math.Clamp(RoundTopLeft(PrimMinY), 0, Height);
        int PrimLowerBound = Math.Clamp(RoundTopLeft(PrimMaxY), 0, Height);

        Scanline[] Scanlines = new Scanline[PrimLowerBound - PrimUpperBound];

        Trace(ScreenTriangle.a, ScreenTriangle.b, PrimUpperBound, Scanlines);
        Trace(ScreenTriangle.b, ScreenTriangle.c, PrimUpperBound, Scanlines);
        Trace(ScreenTriangle.c, ScreenTriangle.a, PrimUpperBound, Scanlines);

        Scan(PrimUpperBound, PrimLowerBound, Scanlines, ViewTriangle, RenderTarget, Shader);
    }

    void Trace(Vector2 Start, Vector2 End, int PrimUpperBound, Scanline[] Scanlines)
    {
        Vector2 TracePath = End - Start;

        if (TracePath.Y == 0) return;

        float SlopeX = TracePath.X / TracePath.Y;

        int TraceUpperBound = Math.Clamp(RoundTopLeft(Min(Start.Y, End.Y)), 0, Height);
        int TraceLowerBound = Math.Clamp(RoundTopLeft(Max(Start.Y, End.Y)), 0, Height);

        int TraceLength = TraceLowerBound - TraceUpperBound;

        if (TracePath.Y < 0)
        {
            float OffsetX = Start.X + (TraceLowerBound - 0.5f - Start.Y) * SlopeX;
            int ScanlineIndex = TraceLowerBound - PrimUpperBound - 1;

            for (int i = 0; i < TraceLength; i++)
            {
                Scanlines[ScanlineIndex].LeftBound = Math.Clamp(RoundTopLeft(OffsetX), 0, Width);
                OffsetX -= SlopeX;
                ScanlineIndex--;
            }
        }
        else
        {
            float OffsetX = Start.X + (TraceUpperBound + 0.5f - Start.Y) * SlopeX;
            int ScanlineIndex = TraceUpperBound - PrimUpperBound;

            for (int i = 0; i < TraceLength; i++)
            {
                Scanlines[ScanlineIndex].RightBound = Math.Clamp(RoundTopLeft(OffsetX), 0, Width);
                OffsetX += SlopeX;
                ScanlineIndex++;
            }
        }
    }

    public abstract Vector2 Project(Vector3 Point);

    public abstract void Scan<TShader>(int UpperBound, int LowerBound, Scanline[] Scanlines, SpatialPrimitive ViewTriangle, Canvas RenderTarget, TShader Shader) where TShader : struct, IShader;
}

public struct Scanline
{
    public int LeftBound;
    public int RightBound;

    public override string ToString() => $"{LeftBound}, {RightBound}";
}