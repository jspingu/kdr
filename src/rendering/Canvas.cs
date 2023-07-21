using static SDL2.SDL;
using static System.MathF;
using static MathUtil;
using System.Numerics;
using System.Runtime.InteropServices;
public class Canvas
{
    int[] DiffuseBuffer;
    int Width, Height, Length;

    public Canvas(int Width, int Height)
    {
        this.Width = Width;
        this.Height = Height;
        this.Length = Width * Height;
        this.DiffuseBuffer = new int[Width * Height];
    }

    public void DrawPrimitive(Primitive Prim, Shader Shader)
    {
        Vector2 CWNormal = new Vector2(-Prim.a.Y, Prim.a.X);
        float AFirst = Vector2.Dot(Prim.b, CWNormal);

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

        float PrimMinY = Prim.Origin.Y + Min(Min(Prim.a.Y, 0), Prim.b.Y);
        float PrimMaxY = Prim.Origin.Y + Max(Max(Prim.a.Y, 0), Prim.b.Y);

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

        Scan(PrimUpperBound, PrimLowerBound, Scanlines, Shader);
    }

    void Trace(Vector2 Start, Vector2 End, int PrimUpperBound, bool HasLowest, Scanline[] Scanlines)
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

    void Scan(int UpperBound, int LowerBound, Scanline[] Scanlines, Shader Shader)
    {
        for (int y = Math.Clamp(UpperBound, 0, Height); y < Math.Clamp(LowerBound, 0, Height); y++)
        {
            Scanline CurrentScan = Scanlines[y - UpperBound];
            int Offset = y * Width;
            
            for (int x = Math.Clamp(CurrentScan.LeftBound, 0, Width); x < Math.Clamp(CurrentScan.RightBound, 0, Width); x++)
            {
                DiffuseBuffer[Offset + x] = Shader.Compute(x, y);
            }
        }
    }

    public void Clear() => Array.Clear(DiffuseBuffer);

    public unsafe void PushToSurface(IntPtr Surface) => Marshal.Copy(DiffuseBuffer, 0, ((SDL_Surface*)Surface)->pixels, Length);
}

public struct Scanline
{
    public int LeftBound;
    public int RightBound;

    public override string ToString() => $"{LeftBound}, {RightBound}";
}