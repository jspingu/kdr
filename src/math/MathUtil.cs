using System.Numerics;
using static System.MathF;

public static class MathUtil
{    
    public static float Frac(float f) => f - (int) f;
    
    public static float ModFrac(float f) => f - Floor(f);

    public static int RoundTopLeft(float f) => (int) Ceiling(f - 0.5f);
}

public static class VectorExtensions
{
	public static Vector3 Rotated(this Vector3 vector, Vector3 axis, float angle)
	{
		Vector3 axisProjection = Vector3.Dot(vector, axis) * axis;
		Vector3 axisPerpendicular = vector - axisProjection;
		
		return axisProjection + Cos(angle) * axisPerpendicular + Sin(angle) * Vector3.Cross(axis, axisPerpendicular);
	}

    public static Vector2 ClockwiseNormal(this Vector2 v) => new Vector2(-v.Y, v.X);
    public static Vector2 CounterClockwiseNormal(this Vector2 v) => new Vector2(v.Y, -v.X);

    public static Vector2 ToVector2(this Vector3 v) => new Vector2(v.X, v.Y);
}