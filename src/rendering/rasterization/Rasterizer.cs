using static System.MathF;
using static MathUtil;
using System.Numerics;

public abstract class Rasterizer
{
    public int Width, Height;
    public float Near;
    public Vector2 Midpoint;

    public Rasterizer(int width, int height, float near)
    {
        Width = width;
        Height = height;

        Near = near;

        Midpoint = new Vector2(width, height) / 2f;
    }

    public void DrawModel<TShader>(Vector3[] viewSpaceVertices, Vector2[] textureVertices, IndexedFace[] faces, Canvas renderTarget, TShader shader) where TShader : struct, IShader
    {
        Vector2[] screenSpaceVertices = new Vector2[viewSpaceVertices.Length];

        for(int i = 0; i < screenSpaceVertices.Length; i++)
        {
            if (viewSpaceVertices[i].Z < Near) continue;
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
                viewSpaceVertices[face.V3]
            };

            SpatialPrimitive viewTriangle = new(
                new Vertex(viewTriangleVertices[0], textureVertices[face.T1]),
                new Vertex(viewTriangleVertices[1], textureVertices[face.T2]),
                new Vertex(viewTriangleVertices[2], textureVertices[face.T3])
            );

            int behindNearCount = 0;
            
            int indexBehind = 3;
            int indexFront = 3;

            for(int i = 0; i < 3; i++)
            {
                if(viewTriangleVertices[i].Z < Near)
                {
                    behindNearCount++;
                    indexFront -= i;
                }
                else
                {
                    indexBehind -= i;
                }
            }

            switch(behindNearCount)
            {
                case 0:
                {
                    Primitive screenTriangle = new(
                        screenTriangleVertices[0],
                        screenTriangleVertices[1],
                        screenTriangleVertices[2]
                    );

                    DrawTriangle(screenTriangle, viewTriangle, renderTarget, shader);

                    break;
                }
                
                case 1:
                {
                    int nextIndex = (indexBehind + 1) % 3;
                    int previousIndex = (indexBehind + 2) % 3;

                    Vector2 pivot = Project(IntersectNear(viewTriangleVertices[indexBehind], viewTriangleVertices[nextIndex]));
                    Vector2 end = Project(IntersectNear(viewTriangleVertices[indexBehind], viewTriangleVertices[previousIndex]));

                    Primitive screenTriangle1 = new(
                        pivot,
                        screenTriangleVertices[nextIndex],
                        screenTriangleVertices[previousIndex]
                    );

                    Primitive screenTriangle2 = new(
                        pivot,
                        screenTriangleVertices[previousIndex],
                        end
                    );

                    DrawTriangle(screenTriangle1, viewTriangle, renderTarget, shader);
                    DrawTriangle(screenTriangle2, viewTriangle, renderTarget, shader);

                    break;
                }

                case 2:
                {
                    int nextIndex = (indexFront + 1) % 3;
                    int previousIndex = (indexFront + 2) % 3;

                    Primitive screenTriangle = new(
                        screenTriangleVertices[indexFront],
                        Project(IntersectNear(viewTriangleVertices[indexFront], viewTriangleVertices[nextIndex])),
                        Project(IntersectNear(viewTriangleVertices[indexFront], viewTriangleVertices[previousIndex]))
                    );

                    DrawTriangle(screenTriangle, viewTriangle, renderTarget, shader);

                    break;
                }
            }
        }
    }

    Vector3 IntersectNear(Vector3 start, Vector3 end) => start + (end - start) * (Near - start.Z) / (end.Z - start.Z);

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