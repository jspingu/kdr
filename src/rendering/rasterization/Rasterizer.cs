using static System.MathF;
using static MathUtil;
using System.Numerics;

public abstract class Rasterizer
{
    public int Width, Height;
    public Vector2 Midpoint;

    public Rasterizer(int width, int height)
    {
        Width = width;
        Height = height;

        Midpoint = new Vector2(width, height) / 2f;
    }

    public void DrawModel<TShader>(Vector3[] viewSpaceVertices, Vector2[] textureVertices, IndexedFace[] faces, Canvas renderTarget, TShader shader) where TShader : struct, IShader
    {
        Vector2[] screenSpaceVertices = new Vector2[viewSpaceVertices.Length];

        for(int i = 0; i < screenSpaceVertices.Length; i++)
        {
            screenSpaceVertices[i] = Project(viewSpaceVertices[i]);
        }

        foreach(IndexedFace face in faces)
        {
            Primitive screenTriangle = new(
                screenSpaceVertices[face.V1],
                screenSpaceVertices[face.V2],
                screenSpaceVertices[face.V3]
            );

            SpatialPrimitive viewTriangle = new(
                new Vertex(viewSpaceVertices[face.V1], textureVertices[face.T1]),
                new Vertex(viewSpaceVertices[face.V2], textureVertices[face.T2]),
                new Vertex(viewSpaceVertices[face.V3], textureVertices[face.T3])
            );

            DrawTriangle(screenTriangle, viewTriangle, renderTarget, shader);
        }
    }

    public void DrawTriangle<TShader>(Primitive screenTriangle, SpatialPrimitive viewTriangle, Canvas renderTarget, TShader shader) where TShader : struct, IShader
    {
        Vector2 AToB = screenTriangle.B - screenTriangle.A;
        Vector2 AToC = screenTriangle.C - screenTriangle.A;

        float AToBFirst = Vector2.Dot(AToB.ClockwiseNormal(), AToC);

        if (AToBFirst <= 0) return; // Vertices not in clockwise order or triangle sides are parallel

        float primMinY = screenTriangle.A.Y + Min(Min(AToB.Y, 0), AToC.Y);
        float primMaxY = screenTriangle.A.Y + Max(Max(AToB.Y, 0), AToC.Y);

        int primUpperBound = Math.Clamp(RoundTopLeft(primMinY), 0, Height);
        int primLowerBound = Math.Clamp(RoundTopLeft(primMaxY), 0, Height);

        Scanline[] scanlines = new Scanline[primLowerBound - primUpperBound];

        Trace(screenTriangle.A, screenTriangle.B, primUpperBound, scanlines);
        Trace(screenTriangle.B, screenTriangle.C, primUpperBound, scanlines);
        Trace(screenTriangle.C, screenTriangle.A, primUpperBound, scanlines);

        Scan(primUpperBound, primLowerBound, scanlines, viewTriangle, renderTarget, shader);
    }

    void Trace(Vector2 start, Vector2 end, int primUpperBound, Scanline[] scanlines)
    {
        Vector2 tracePath = end - start;

        if (tracePath.Y == 0) return;

        float slopeX = tracePath.X / tracePath.Y;

        int traceUpperBound = Math.Clamp(RoundTopLeft(Min(start.Y, end.Y)), 0, Height);
        int traceLowerBound = Math.Clamp(RoundTopLeft(Max(start.Y, end.Y)), 0, Height);

        int traceLength = traceLowerBound - traceUpperBound;

        if (tracePath.Y < 0)
        {
            float offsetX = start.X + (traceLowerBound - 0.5f - start.Y) * slopeX;
            int scanlineIndex = traceLowerBound - primUpperBound - 1;

            for (int i = 0; i < traceLength; i++)
            {
                scanlines[scanlineIndex].LeftBound = Math.Clamp(RoundTopLeft(offsetX), 0, Width);
                offsetX -= slopeX;
                scanlineIndex--;
            }
        }
        else
        {
            float offsetX = start.X + (traceUpperBound + 0.5f - start.Y) * slopeX;
            int scanlineIndex = traceUpperBound - primUpperBound;

            for (int i = 0; i < traceLength; i++)
            {
                scanlines[scanlineIndex].RightBound = Math.Clamp(RoundTopLeft(offsetX), 0, Width);
                offsetX += slopeX;
                scanlineIndex++;
            }
        }
    }

    public abstract Vector2 Project(Vector3 point);

    public abstract void Scan<TShader>(int upperBound, int lowerBound, Scanline[] scanlines, SpatialPrimitive viewTriangle, Canvas renderTarget, TShader shader) where TShader : struct, IShader;
}

public struct Scanline
{
    public int LeftBound;
    public int RightBound;

    public override string ToString() => $"{LeftBound}, {RightBound}";
}