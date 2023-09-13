using System.Numerics;
using static System.MathF;

public static class MathUtil
{
    public static unsafe int Blint(bool b) => *(byte*)&b;
    
    public static float Frac(float f) => f - (int) f;
    
    public static float ModFrac(float f) => f - Floor(f);

    public static int RoundTopLeft(float f) => (int)Floor(f) + Blint(ModFrac(f) > 0.5f);
}

public static class VectorExtensions
{
	public static Vector3 Rotated(this Vector3 Vector, Vector3 Axis, float Angle)
	{
		Vector3 AxisProjection = Vector3.Dot(Vector, Axis) * Axis;
		Vector3 AxisPerpendicular = Vector - AxisProjection;
		
		return AxisProjection + Cos(Angle) * AxisPerpendicular + Sin(Angle) * Vector3.Cross(Axis, AxisPerpendicular);
	}

    public static Vector2 ClockwiseNormal(this Vector2 v) => new Vector2(-v.Y, v.X);
    public static Vector2 CounterClockwiseNormal(this Vector2 v) => new Vector2(v.Y, -v.X);

    public static Vector2 ToVector2(this Vector3 v) => new Vector2(v.X, v.Y);
}