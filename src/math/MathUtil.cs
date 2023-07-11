using static System.MathF;
public static class MathUtil
{
    public static int ThresholdRound(float Value, float Threshold, bool Inclusive)
    {
        if (Inclusive) return Mod(Value, 1) >= Threshold ? (int) Ceiling(Value) : (int) Floor(Value);
        else return Mod(Value, 1) > Threshold ? (int) Ceiling(Value) : (int) Floor(Value);
    }

    public static float Mod(float Dividend, float Divisor)
    {
        float Remainder = Dividend % Divisor;
        return Remainder < 0 ? Remainder + Divisor : Remainder;
    }
}