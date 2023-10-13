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

    float TanHalfFOV;
    
    Vector2 Midpoint, ScreenToProjectionPlane;

    public Canvas(int Width, int Height)
    {
        this.Width = Width;
        this.Height = Height;
        this.Length = Width * Height;
        this.Pitch = Width * sizeof(int);

        this.Midpoint = new Vector2(Width, Height) / 2f;

        this.TanHalfFOV = Tan(PI/2f / 2f);
        this.ScreenToProjectionPlane = TanHalfFOV / Midpoint.X * new Vector2(1, -1);
        
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

        int PrimUpperBound = Math.Clamp(RoundTopLeft(PrimMinY), 0, Height);
        int PrimLowerBound = Math.Clamp(RoundTopLeft(PrimMaxY), 0, Height);

        Scanline[] Scanlines = new Scanline[PrimLowerBound - PrimUpperBound];

        for (int i = 0; i < 3; i++)
        {
            Trace(ClockwiseVertices[i], ClockwiseVertices[i + 1], PrimUpperBound, Scanlines);
        }

        ScanPerspective(PrimUpperBound, PrimLowerBound, Scanlines, ViewTriangle, new Transform2(new Basis2(AToB, AToC), ScreenTriangle.a), Shader);
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

    void ScanPerspective<TShader>(int UpperBound, int LowerBound, Scanline[] Scanlines, SpatialPrimitive ViewTriangle, Transform2 TriangleTransform, TShader Shader) where TShader : struct, IShader
    {
        Vector3 ViewAB = ViewTriangle.v2.Position - ViewTriangle.v1.Position;
        Vector3 ViewAC = ViewTriangle.v3.Position - ViewTriangle.v1.Position;

        Vector3 PerpAB = ViewAB.LengthSquared() * ViewAC - Vector3.Dot(ViewAC, ViewAB) * ViewAB;
        Vector3 PerpAC = ViewAC.LengthSquared() * ViewAB - Vector3.Dot(ViewAB, ViewAC) * ViewAC;

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

        float NormalDisplacement = Vector3.Dot(ViewTriangle.v1.Position, ViewTriangle.Normal);

        Parallel.For(UpperBound, LowerBound, (y) => {
            Scanline CurrentScan = Scanlines[y - UpperBound];
            int Offset = y * Width;
            
            for (int x = CurrentScan.LeftBound; x < CurrentScan.RightBound; x++)
            {
                Vector2 ProjPlane = (new Vector2(x, y) + new Vector2(0.5f, 0.5f) - Midpoint) * ScreenToProjectionPlane;

                float FragmentDepth = NormalDisplacement / Vector3.Dot(new Vector3(ProjPlane, 1), ViewTriangle.Normal);

                if (FragmentDepth > DepthBuffer[Offset + x]) continue;
                DepthBuffer[Offset + x] = FragmentDepth;

                Vector2 FragmentTexCoord = ViewTriangle.v1.TexCoord + (TextureTransform * (new Vector3(ProjPlane, 1) * FragmentDepth - ViewTriangle.v1.Position)).ToVector2();

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

    void ScanOrthographic<TShader>(int UpperBound, int LowerBound, Scanline[] Scanlines, SpatialPrimitive ViewTriangle, Transform2 TriangleTransform, TShader Shader) where TShader : struct, IShader
    {
        Basis2 InverseTransform = TriangleTransform.Basis.Inversed();

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

            Vector2 PixelCenter = new Vector2(CurrentScan.LeftBound - 1, y) + new Vector2(0.5f, 0.5f) - TriangleTransform.Translation;

            Vector2 FragmentTexCoord = ViewTriangle.v1.TexCoord + TextureTransform * PixelCenter;
            float FragmentDepth = ViewTriangle.v1.Position.Z + Vector2.Dot(DepthTransform, PixelCenter);
            
            for (int x = CurrentScan.LeftBound; x < CurrentScan.RightBound; x++)
            {
                FragmentTexCoord += TextureTransform.i;
                FragmentDepth += DepthTransform.X;

                if (FragmentDepth > DepthBuffer[Offset + x]) continue;
                DepthBuffer[Offset + x] = FragmentDepth;

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