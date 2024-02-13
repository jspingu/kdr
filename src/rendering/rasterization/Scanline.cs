namespace KDR;

public struct Scanline
{
    public int LeftBound;
    public int RightBound;

    public override string ToString() => $"{LeftBound}, {RightBound}";
}
