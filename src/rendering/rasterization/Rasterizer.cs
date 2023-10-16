using static System.MathF;
using static MathUtil;
using System.Numerics;

public abstract class Rasterizer
{
    public int Width, Height;
    public float Near, Far;
    public Vector2 Midpoint;

    public Rasterizer(int width, int height, float near, float far)
    {
        Width = width;
        Height = height;

        Near = near;
        Far = far;

        Midpoint = new Vector2(width, height) / 2f;
    }

    static Vector3 IntersectViewAlignedPlane(Vector3 start, Vector3 end, float depth)
    {
        Vector3 path = end - start;
        return start + path / path.Z * (depth - start.Z);
    }

    public void DrawModel<TShader>(Vector3[] viewSpaceVertices, Vector2[] textureVertices, IndexedFace[] faces, Canvas renderTarget, TShader shader) where TShader : struct, IShader
    {
        Vector2[] screenSpaceVertices = new Vector2[viewSpaceVertices.Length];

        for(int i = 0; i < screenSpaceVertices.Length; i++)
        {
            if (viewSpaceVertices[i].Z > Far || viewSpaceVertices[i].Z < Near) continue;
            screenSpaceVertices[i] = Project(viewSpaceVertices[i]);
        }

        foreach(IndexedFace face in faces)
        {
            Vector2[] screenTriangleVertices = new Vector2[]
            {
                screenSpaceVertices[face.V1],
                screenSpaceVertices[face.V2],
                screenSpaceVertices[face.V3]
            };

            Vector3[] viewTriangleVertices = new Vector3[]
            {
                viewSpaceVertices[face.V1],
                viewSpaceVertices[face.V2], 
                viewSpaceVertices[face.V3],
                viewSpaceVertices[face.V1]
            };

            SpatialPrimitive viewTriangle = new(
                new Vertex(viewTriangleVertices[0], textureVertices[face.T1]),
                new Vertex(viewTriangleVertices[1], textureVertices[face.T2]),
                new Vertex(viewTriangleVertices[2], textureVertices[face.T3])
            );

            int vertexCount = 0;
            Vector2[] clippedVertices = new Vector2[5];

            for(int i = 0; i < 3; i++)
            {
                Vector3 current = viewTriangleVertices[i];
                Vector3 next = viewTriangleVertices[i + 1];

                float firstDepth = Near;
                float secondDepth = Far;
                
                bool firstCondition, secondCondition;

                if (current.Z <= Far)
                {
                    secondCondition = next.Z > Far;

                    if (current.Z >= Near)
                    {
                        clippedVertices[vertexCount++] = screenTriangleVertices[i];
                        firstCondition = next.Z < Near;
                    }
                    else firstCondition = next.Z > Near;
                }
                else
                {
                    firstDepth = Far;
                    secondDepth = Near;

                    firstCondition = next.Z < Far;
                    secondCondition = next.Z < Near;
                }

                if (firstCondition) clippedVertices[vertexCount++] = Project(IntersectViewAlignedPlane(current, next, firstDepth));
                if (secondCondition) clippedVertices[vertexCount++] = Project(IntersectViewAlignedPlane(current, next, secondDepth));
            }

            for(int i = 0; i < vertexCount - 2; i++)
            {
                Primitive screenTriangle = new(
                    clippedVertices[0],
                    clippedVertices[i + 1],
                    clippedVertices[i + 2]
                );

                DrawTriangle(screenTriangle, viewTriangle, renderTarget, shader);
            }
        }
    }

    void DrawTriangle<TShader>(Primitive screenTriangle, SpatialPrimitive viewTriangle, Canvas renderTarget, TShader shader) where TShader : struct, IShader
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