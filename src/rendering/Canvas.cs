using static SDL2.SDL;
using static System.MathF;
using static MathUtil;
using static ShaderUtil;
using System.Numerics;
using System.Runtime.InteropServices;

public class Canvas
{
    int[] DiffuseBuffer;
    int Width, Height, Length, Pitch;

    Vector2 Midpoint;

    public Canvas(int Width, int Height)
    {
        this.Width = Width;
        this.Height = Height;
        this.Length = Width * Height;
        this.Pitch = Width * sizeof(int);

        this.Midpoint = new Vector2(Width, Height) / 2f;
        
        this.DiffuseBuffer = new int[Width * Height];
    }

    public void DrawSpatialPrimitive(SpatialPrimitive ViewTriangle, Shader Shader)
    {
        if (ViewTriangle.Normal.Z > 0) return;

        // Orthogonal projection, no near plane clipping
        Primitive ScreenTriangle = new Primitive(
            ViewTriangle.v1.Position.ToVector2() + Midpoint,
            ViewTriangle.v2.Position.ToVector2() + Midpoint,
            ViewTriangle.v3.Position.ToVector2() + Midpoint
        );

        Vector2 AToB = ScreenTriangle.b - ScreenTriangle.a;
        Vector2 AToC = ScreenTriangle.c - ScreenTriangle.a;

        float AToBFirst = Vector2.Dot(AToB.ClockwiseNormal(), AToC);

        if (AToBFirst == 0) return; // Triangle sides are parallel

        Vector2 SecondVertex, ThirdVertex;

        if(AToBFirst > 0)
        {
            SecondVertex = ScreenTriangle.b;
            ThirdVertex = ScreenTriangle.c;
        }
        else
        {
            SecondVertex = ScreenTriangle.c;
            ThirdVertex = ScreenTriangle.b;
        }

        Vector2[] ClockwiseVertices = new Vector2[] {ScreenTriangle.a, SecondVertex, ThirdVertex, ScreenTriangle.a};

        float PrimMinY = ScreenTriangle.a.Y + Min(Min(AToB.Y, 0), AToC.Y);
        float PrimMaxY = ScreenTriangle.a.Y + Max(Max(AToB.Y, 0), AToC.Y);

        int PrimUpperBound = iRound(PrimMinY, false);
        int PrimLowerBound = iRound(PrimMaxY, false);

        Scanline[] Scanlines = new Scanline[PrimLowerBound - PrimUpperBound];

        for (int i = 0; i < 3; i++)
        {
            Vector2 TraceStart = ClockwiseVertices[i];
            Vector2 TraceEnd = ClockwiseVertices[i + 1];
            bool TraceHasLowest = TraceStart.Y == PrimMaxY || TraceEnd.Y == PrimMaxY;
            Trace(TraceStart, TraceEnd, PrimUpperBound, TraceHasLowest, Scanlines);
        }

        Scan(PrimUpperBound, PrimLowerBound, Scanlines, ViewTriangle, new Transform2(new Basis2(AToB, AToC), ScreenTriangle.a), Shader);
    }

    static void Trace(Vector2 Start, Vector2 End, int PrimUpperBound, bool HasLowest, Scanline[] Scanlines)
    {
        Vector2 TracePath = End - Start;

        if (TracePath.Y == 0) return;

        bool Upwards = TracePath.Y < 0;

        float SlopeX = TracePath.X / TracePath.Y;
        float OffsetX = Start.X + Frac((Upwards ? -0.5f : 1.5f) - ModFrac(Start.Y)) * SlopeX;
        if (ModFrac(Start.Y) == 0.5f && HasLowest && Upwards) OffsetX -= SlopeX;

        int TraceUpperBound = iRound(Min(Start.Y, End.Y), false);
        int TraceLowerBound = iRound(Max(Start.Y, End.Y), !HasLowest);

        int TraceLength = TraceLowerBound - TraceUpperBound;

        if (Upwards)
        {
            int ScanlineIndex = TraceLowerBound - PrimUpperBound - 1;

            for (int i = 0; i < TraceLength; i++)
            {
                Scanlines[ScanlineIndex].LeftBound = iRound(OffsetX, false);
                OffsetX -= SlopeX;
                ScanlineIndex--;
            }
        }
        else
        {
            int ScanlineIndex = TraceUpperBound - PrimUpperBound;

            for (int i = 0; i < TraceLength; i++)
            {
                Scanlines[ScanlineIndex].RightBound = iRound(OffsetX, false);
                OffsetX += SlopeX;
                ScanlineIndex++;
            }
        }
    }

    void Scan(int UpperBound, int LowerBound, Scanline[] Scanlines, SpatialPrimitive ViewTriangle, Transform2 TriangleTransform, Shader Shader)
    {
        Vector2 iNormal = TriangleTransform.Basis.i.ClockwiseNormal();
        Vector2 jNormal = TriangleTransform.Basis.j.ClockwiseNormal();

        float DivAB = 1 / Vector2.Dot(-TriangleTransform.Basis.i, jNormal);
        float DivAC = 1 / Vector2.Dot(-TriangleTransform.Basis.j, iNormal);

        Basis2 InverseTransform = new Basis2(
            new Vector2(
                jNormal.X * DivAB,
                iNormal.X * DivAC
            ),
            new Vector2(
                jNormal.Y * DivAB,
                iNormal.Y * DivAC
            )
        );

        Parallel.For(Math.Clamp(UpperBound, 0, Height), Math.Clamp(LowerBound, 0, Height), (y) => {
            Scanline CurrentScan = Scanlines[y - UpperBound];
            int Offset = y * Width;
            
            for (int x = Math.Clamp(CurrentScan.LeftBound, 0, Width); x < Math.Clamp(CurrentScan.RightBound, 0, Width); x++)
            {
                Vector2 TriangleOffset = TriangleTransform.Translation - (new Vector2(x, y) + new Vector2(0.5f, 0.5f));
                Vector2 AffineCoordinates = InverseTransform * TriangleOffset;

                Vector3 BarycentricWeights = new Vector3(
                    1 - AffineCoordinates.X - AffineCoordinates.Y,
                    AffineCoordinates.X,
                    AffineCoordinates.Y
                );

                float FragmentDepth = WeighBarycentric(
                    BarycentricWeights, 
                    ViewTriangle.v1.Position.Z, 
                    ViewTriangle.v2.Position.Z, 
                    ViewTriangle.v3.Position.Z
                );

                Vector2 FragmentTexCoord = WeighBarycentric(
                    BarycentricWeights, 
                    ViewTriangle.v1.TexCoord, 
                    ViewTriangle.v2.TexCoord, 
                    ViewTriangle.v3.TexCoord
                );

                ShaderParam Fragment = new ShaderParam(
                    x, y,
                    BarycentricWeights,
                    FragmentDepth,
                    FragmentTexCoord,
                    ViewTriangle.Normal
                );

                DiffuseBuffer[Offset + x] = Shader.Compute(Fragment);
            }
        });
    }

    public void Clear() => Array.Clear(DiffuseBuffer);

    public unsafe void PushToSurface(IntPtr Surface) => Marshal.Copy(DiffuseBuffer, 0, ((SDL_Surface*)Surface)->pixels, Length);

    public unsafe void UploadToSDLTexture(IntPtr Texture)
    {
        fixed(void* BufferPtr = &DiffuseBuffer[0])
        {
            SDL_UpdateTexture(Texture, 0, (IntPtr)BufferPtr, Pitch);
        }
    }
}

public struct Scanline
{
    public int LeftBound;
    public int RightBound;

    public override string ToString() => $"{LeftBound}, {RightBound}";
}