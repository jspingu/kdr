using static System.MathF;
public static class MathUtil
{
    public static float Frac(float f) => f - (int) f;
    public static float ModFrac(float f) => f - Floor(f);
}