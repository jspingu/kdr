using static System.MathF;

public static class MathUtil
{
    public static unsafe byte Blint(bool b) => *(byte*)&b;
    
    public static float Frac(float f) => f - (int) f;
    
    public static float ModFrac(float f) => f - Floor(f);

    public static int iRound(float f, bool IncludeMid) => (int)Floor(f) + Blint(ModFrac(f) > 0.5f || IncludeMid && ModFrac(f) == 0.5f);
}