using static SDL2.SDL;
using static System.MathF;
using static MathUtil;
public class Canvas
{
    FragmentData[] FragmentBuffer;
    int Width, Height;

    public Canvas(int Width, int Height)
    {
        this.Width = Width;
        this.Height = Height;
        this.FragmentBuffer = new FragmentData[Width * Height];
    }

    public void DrawPrimitive(Primitive Prim, Shader Shader)
    {
        Vector2 CWNormal = new Vector2(-Prim.a.y, Prim.a.x);
        float AFirst = Prim.b.Dot(CWNormal);

        if (AFirst == 0) return;

        Vector2 First, Second;

        if(AFirst > 0)
        {
            First = Prim.a;
            Second = Prim.b;
        }
        else
        {
            First = Prim.b;
            Second = Prim.a;
        }

        Vector2[] ClockwiseVertices = new Vector2[] {Prim.Origin, Prim.Origin + First, Prim.Origin + Second, Prim.Origin};

        float PrimMinY = Prim.Origin.y + Min(Min(Prim.a.y, 0), Prim.b.y);
        float PrimMaxY = Prim.Origin.y + Max(Max(Prim.a.y, 0), Prim.b.y);

        int PrimUpperBound = (int) Round(PrimMinY, MidpointRounding.ToNegativeInfinity);
        int PrimLowerBound = (int) Round(PrimMaxY, MidpointRounding.ToNegativeInfinity);

        Scanline[] Scanlines = new Scanline[PrimLowerBound - PrimUpperBound];

        for (int i = 0; i < 3; i++)
        {
            Vector2 TraceStart = ClockwiseVertices[i];
            Vector2 TraceEnd = ClockwiseVertices[i + 1];
            bool TraceHasLowest = TraceStart.y == PrimMaxY || TraceEnd.y == PrimMaxY;
            Trace(TraceStart, TraceEnd, PrimUpperBound, TraceHasLowest, Scanlines);
        }

        Scan(PrimUpperBound, PrimLowerBound, Scanlines, Shader);
    }

    void Trace(Vector2 Start, Vector2 End, int PrimUpperBound, bool HasLowest, Scanline[] Scanlines)
    {
        Vector2 TracePath = End - Start;

        if (TracePath.y == 0) return;

        bool Upwards = TracePath.y < 0;

        float SlopeX = TracePath.x / TracePath.y;
        float OffsetX = Start.x + Frac((Upwards ? -0.5f : 1.5f) - ModFrac(Start.y)) * SlopeX;
        if (HasLowest && Upwards && ModFrac(Start.y) == 0.5f) OffsetX -= SlopeX;

        int TraceUpperBound = (int) Round(Min(Start.y, End.y), MidpointRounding.ToNegativeInfinity);
        int TraceLowerBound = (int) Round(Max(Start.y, End.y), HasLowest ? MidpointRounding.ToNegativeInfinity : MidpointRounding.ToPositiveInfinity);

        int TraceLength = TraceLowerBound - TraceUpperBound;

        if (Upwards)
        {
            int ScanlineIndex = TraceLowerBound - PrimUpperBound - 1;

            for (int i = 0; i < TraceLength; i++)
            {
                Scanlines[ScanlineIndex].LeftBound = (int) Round(OffsetX, MidpointRounding.ToNegativeInfinity);
                OffsetX -= SlopeX;
                ScanlineIndex--;
            }
        }
        else
        {
            int ScanlineIndex = TraceUpperBound - PrimUpperBound;

            for (int i = 0; i < TraceLength; i++)
            {
                Scanlines[ScanlineIndex].RightBound = (int) Round(OffsetX, MidpointRounding.ToNegativeInfinity);
                OffsetX += SlopeX;
                ScanlineIndex++;
            }
        }
    }

    void Scan(int UpperBound, int LowerBound, Scanline[] Scanlines, Shader Shader)
    {
        for (int y = Math.Clamp(UpperBound, 0, Height); y < Math.Clamp(LowerBound, 0, Height); y++)
        {
            Scanline CurrentScan = Scanlines[y - UpperBound];
            int Offset = y * Width;
            
            for (int x = Math.Clamp(CurrentScan.LeftBound, 0, Width); x < Math.Clamp(CurrentScan.RightBound, 0, Width); x++)
            {
                FragmentBuffer[Offset + x] = Shader.Compute(x, y);
            }
        }
    }

    public void Clear() => Array.Clear(FragmentBuffer);

    public unsafe void PushToSurface(IntPtr Surface)
    {
        SDL_Surface* PSurface = (SDL_Surface*) Surface;
        uint* Start = (uint*) PSurface->pixels;

        for (int i = 0; i < Width * Height; i++)
        {
            byte* PixelPtr = (byte*)Start;

            PixelPtr[2] = FragmentBuffer[i].FragColor.r;
            PixelPtr[1] = FragmentBuffer[i].FragColor.g;
            PixelPtr[0] = FragmentBuffer[i].FragColor.b;

            Start++;
        }
    }
}

public struct Color
{
    public byte r, g, b;

    public Color(byte r, byte g, byte b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
}

public struct FragmentData
{
    public Color FragColor;
    public float Depth;

    public FragmentData(Color FragColor, float Depth)
    {
        this.FragColor = FragColor;
        this.Depth = Depth;
    }
}

public struct Scanline
{
    public int LeftBound;
    public int RightBound;

    public override string ToString() => $"{LeftBound}, {RightBound}";
}