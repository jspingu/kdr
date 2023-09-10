using static SDL2.SDL;
using static System.MathF;
using static MathUtil;
using System.Numerics;
using System.Runtime.InteropServices;

public class Canvas
{
    int[] OutputBuffer;
    float[] DepthBuffer;

    int Width, Height, Length, Pitch;

    float TanHalfFOV, ScreenToProjectionPlane;

    Vector2 Midpoint;

    public Canvas(int Width, int Height)
    {
        this.Width = Width;
        this.Height = Height;
        this.Length = Width * Height;
        this.Pitch = Width * sizeof(int);

        this.Midpoint = new Vector2(Width, Height) / 2f;

        this.TanHalfFOV = Tan(PI/4f);
        this.ScreenToProjectionPlane = TanHalfFOV / Midpoint.X;
        
        this.OutputBuffer = new int[Width * Height];
        this.DepthBuffer = new float[Width * Height];
    }

    public void DrawSpatialPrimitive<TShader>(SpatialPrimitive ViewTriangle, TShader Shader) where TShader : struct, IShader
    {
        if (ViewTriangle.Normal.Z > 0) return;

        Primitive ScreenTriangle = new Primitive(
            ViewTriangle.v1.Position.ToVector2() / (ViewTriangle.v1.Position.Z * TanHalfFOV) * Midpoint.X + Midpoint,
            ViewTriangle.v2.Position.ToVector2() / (ViewTriangle.v2.Position.Z * TanHalfFOV) * Midpoint.X + Midpoint,
            ViewTriangle.v3.Position.ToVector2() / (ViewTriangle.v3.Position.Z * TanHalfFOV) * Midpoint.X + Midpoint
        );

        // Orthogonal projection, no near plane clipping
        // Primitive ScreenTriangle = new Primitive(
        //     ViewTriangle.v1.Position.ToVector2() + Midpoint,
        //     ViewTriangle.v2.Position.ToVector2() + Midpoint,
        //     ViewTriangle.v3.Position.ToVector2() + Midpoint
        // );

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

    void Scan<TShader>(int UpperBound, int LowerBound, Scanline[] Scanlines, SpatialPrimitive ViewTriangle, Transform2 TriangleTransform, TShader Shader) where TShader : struct, IShader
    {
        // Basis2 InverseTransform = TriangleTransform.Basis.Inversed();

        Basis2 InverseTransform = new Basis2(
            (ViewTriangle.v2.Position - ViewTriangle.v1.Position).ToVector2(),
            (ViewTriangle.v3.Position - ViewTriangle.v1.Position).ToVector2()
        ).Inversed();

        Basis2 TextureTransform = new Basis2(
            ViewTriangle.v2.TexCoord - ViewTriangle.v1.TexCoord,
            ViewTriangle.v3.TexCoord - ViewTriangle.v1.TexCoord
        ) * InverseTransform;

        // Vector2 DepthTransform = new Vector2(
        //     ViewTriangle.v2.Position.Z - ViewTriangle.v1.Position.Z,
        //     ViewTriangle.v3.Position.Z - ViewTriangle.v1.Position.Z
        // ) * InverseTransform;

        Parallel.For(Math.Clamp(UpperBound, 0, Height), Math.Clamp(LowerBound, 0, Height), (y) => {
            Scanline CurrentScan = Scanlines[y - UpperBound];
            int Offset = y * Width;

            int ClampedLeft = Math.Clamp(CurrentScan.LeftBound, 0, Width);
            int ClampedRight = Math.Clamp(CurrentScan.RightBound, 0, Width);

            // Vector2 PixelCenter = new Vector2(ClampedLeft - 1, y) + new Vector2(0.5f, 0.5f) - TriangleTransform.Translation;

            // Vector2 FragmentTexCoord = ViewTriangle.v1.TexCoord + TextureTransform * PixelCenter;
            // float FragmentDepth = ViewTriangle.v1.Position.Z + Vector2.Dot(DepthTransform, PixelCenter);
            
            for (int x = ClampedLeft; x < ClampedRight; x++)
            {
                // FragmentTexCoord += TextureTransform.i;
                // FragmentDepth += DepthTransform.X;

                Vector2 ProjPlane = (new Vector2(x, y) + new Vector2(0.5f, 0.5f) - Midpoint) * ScreenToProjectionPlane;
                
                float FragmentDepth = Vector3.Dot(ViewTriangle.v1.Position, ViewTriangle.Normal) / Vector3.Dot(new Vector3(ProjPlane, 1), ViewTriangle.Normal);

                if (FragmentDepth > DepthBuffer[Offset + x]) continue;
                DepthBuffer[Offset + x] = FragmentDepth;

                Vector2 FragmentTexCoord = ViewTriangle.v1.TexCoord + TextureTransform * (ProjPlane * FragmentDepth - ViewTriangle.v1.Position.ToVector2());

                ShaderParam Fragment = new ShaderParam(
                    x, y,
                    FragmentDepth,
                    FragmentTexCoord,
                    ViewTriangle.Normal
                );

                OutputBuffer[Offset + x] = Shader.Compute(Fragment);
            }
        });
    }

    public void Clear()
    {
        Array.Clear(OutputBuffer);
        Array.Fill(DepthBuffer, float.MaxValue);
    }

    public unsafe void PushToSurface(IntPtr Surface) => Marshal.Copy(OutputBuffer, 0, ((SDL_Surface*)Surface)->pixels, Length);

    public unsafe void UploadToSDLTexture(IntPtr Texture)
    {
        fixed(void* BufferPtr = &OutputBuffer[0])
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