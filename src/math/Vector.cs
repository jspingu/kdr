using static System.MathF;

public struct Vector2
{
	public float x;
	public float y;

	public Vector2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
	public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);

	public static Vector2 operator -(Vector2 a) => -1 * a;
	public static Vector2 operator *(Vector2 a, float b) => new Vector2(a.x * b, a.y * b);
	public static Vector2 operator *(float a, Vector2 b) => new Vector2(b.x * a, b.y * a);

	public float Dot(Vector2 a) => x * a.x + y * a.y;

	public float GetLength() => Sqrt(x * x + y * y);

	public Vector2 GetAbs() => new Vector2(Abs(x), Abs(y));

	public override string ToString() => $"Vec2({x}, {y})";
}