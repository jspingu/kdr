using static SDL2.SDL;
using static System.MathF;
using static MathUtil;
public class Canvas
{
    FragmentData[,] FragmentBuffer;
    int Width, Height;

    public Canvas(int Width, int Height)
    {
        this.Width = Width;
        this.Height = Height;
        this.FragmentBuffer = new FragmentData[Width, Height];
    }

    public void DrawPrimitive(Primitive Prim, Shader Shader)
    {
        Vector2 CWNormal = new Vector2(-Prim.a.y, Prim.a.x);
        float AFirst = Prim.b.Dot(CWNormal);

        if (AFirst == 0) return;

        // int LeftBound = ThresholdRound(Prim.Origin.x + Min(Prim.a.x, 0) + Min(Prim.b.x, 0), 0.5f, false);
        // int RightBound = ThresholdRound(Prim.Origin.x + Max(Prim.a.x, 0) + Max(Prim.b.x, 0), 0.5f, false);

        int LeftBound = ThresholdRound(Prim.Origin.x + Min(Min(Prim.a.x, 0), Prim.b.x), 0.5f, false);
        int RightBound = ThresholdRound(Prim.Origin.x + Max(Max(Prim.a.x, 0), Prim.b.x), 0.5f, false);

        Scanline[] Scanlines = new Scanline[RightBound - LeftBound];

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

        // Vector2[] ClockwiseVertices = new Vector2[] {Prim.Origin, Prim.Origin + First, Prim.Origin + First + Second, Prim.Origin + Second};

        Vector2[] ClockwiseVertices = new Vector2[] {Prim.Origin, Prim.Origin + First, Prim.Origin + Second};

        for (int i = 0; i < 3; i++)
        {
            Trace(ClockwiseVertices[i], ClockwiseVertices[(i + 1) % 3], LeftBound, RightBound, Scanlines);
        }

        Scan(LeftBound, RightBound, Scanlines, Shader);
    }

    void Trace(Vector2 Start, Vector2 End, int LeftBound, int RightBound, Scanline[] Scanlines)
    {
        Vector2 TracePath = End - Start; // normal = (y, -x)
        if (TracePath.x == 0) return;

        float Slope = TracePath.y / TracePath.x;

        int TraceLeft = ThresholdRound(Min(Start.x, End.x), 0.5f, false);
        int TraceRight = ThresholdRound(Max(Start.x, End.x), 0.5f, true);
        
        float Offset = Mod(0.5f - Mod(Start.x, 1), 1);

        if (TracePath.x > 0) // Search for upper bounds
        {
            for (int i = 0; i < TraceRight - TraceLeft; i++)
            {
                int ScanlineIndex = TraceLeft - LeftBound + i;
                if (ScanlineIndex == RightBound - LeftBound) break;

                float CurrentY = Start.y + (Offset + i) * Slope;

                if (Mod(CurrentY, 1) > 0.5) Scanlines[ScanlineIndex].SetUpper((int) Ceiling(CurrentY)); // Below pixel center
                else if (Mod(CurrentY, 1) < 0.5) Scanlines[ScanlineIndex].SetUpper((int) Floor(CurrentY)); // Above pixel center
                else if (TracePath.y > 0) Scanlines[ScanlineIndex].SetUpper((int) Ceiling(CurrentY)); // Right edge
                else Scanlines[ScanlineIndex].SetUpper((int) Floor(CurrentY)); // Left edge or top edge
            }
        }
       
        else // Search for lower bounds
        {
            for (int i = 0; i < TraceRight - TraceLeft; i++)
            {
                int ScanlineIndex = TraceRight - LeftBound - i - 1;
                if (ScanlineIndex == RightBound - LeftBound) continue;

                float CurrentY = Start.y + (Offset - 1 - i) * Slope;

                if (Mod(CurrentY, 1) > 0.5) Scanlines[ScanlineIndex].SetLower((int) Ceiling(CurrentY)); // Below pixel center
                else if (Mod(CurrentY, 1) < 0.5) Scanlines[ScanlineIndex].SetLower((int) Floor(CurrentY)); // Above pixel center
                else if (TracePath.y >= 0) Scanlines[ScanlineIndex].SetLower((int) Floor(CurrentY)); // Right edge or bottom edge
                else Scanlines[ScanlineIndex].SetLower((int) Ceiling(CurrentY)); // Left edge
            }
        }
    }

    void Scan(int LeftBound, int RightBound, Scanline[] Scanlines, Shader Shader)
    {
        for (int x = (int) Max(LeftBound, 0); x < (int) Min(RightBound, Width); x++)
        {
            if (Scanlines[x - LeftBound].UpperBound is int UpperInt && Scanlines[x - LeftBound].LowerBound is int LowerInt)
            {
                for (int y = (int) Max(UpperInt, 0); y < (int) Min(LowerInt, Height); y++)
                {
                    FragmentBuffer[x, y] = Shader.Compute(x, y);
                }
            }
        }
    }

    public void Clear()
    {
        // for (int x = 0; x < Width; x++)
        // {
        //     for (int y = 0; y < Height; y++)
        //     {
        //         FragmentBuffer[x, y] = new FragmentData(new Color(0, 0, 0), 0);
        //     }
        // }

        FragmentBuffer = new FragmentData[Width, Height];
    }

    public unsafe void PushToSurface(IntPtr Surface)
    {

        SDL_Surface* PSurface = (SDL_Surface*) Surface;
        uint* Start = (uint*) PSurface->pixels;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                byte* PixelPtr = (byte*)(Start + x + y * Width);

                PixelPtr[2] = FragmentBuffer[x, y].FragColor.r;
                PixelPtr[1] = FragmentBuffer[x, y].FragColor.g;
                PixelPtr[0] = FragmentBuffer[x, y].FragColor.b;

                // Start[x + y * Width] = SDL_MapRGB(PSurface->format, 0, 0, 255);
            }
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
    public int? UpperBound; // Up is negative
    public int? LowerBound; // Down is positive

    public Scanline(int UpperBound, int LowerBound)
    {
        this.UpperBound = UpperBound;
        this.LowerBound = LowerBound;
    }

    public void SetUpper(int UpperCandidate)
    {
        // Upper bound to set is equal to current lower bound, draw nothing on this x-level
        if (LowerBound is int LowerBoundInt && UpperCandidate == LowerBoundInt) UpperBound = LowerBound = null;
        
        // Write upper bound if one doesn't exist or new upper bound is below current upper bound
        else if (UpperBound is null || UpperBound is int UpperBoundInt && UpperCandidate > UpperBoundInt) UpperBound = UpperCandidate;
    }

    public void SetLower(int LowerCandidate)
    {
        // Lower bound to set is equal to current upper bound, draw nothing on this x-level
        if (UpperBound is int UpperBoundInt && LowerCandidate == UpperBoundInt) UpperBound = LowerBound = null;
        
        // Write lower bound if one doesn't exist or new lower bound is above current lower bound
        else if (LowerBound is null || LowerBound is int LowerBoundInt && LowerCandidate < LowerBoundInt) LowerBound = LowerCandidate;
    }

    public override string ToString() => $"{UpperBound}, {LowerBound} | ";
}