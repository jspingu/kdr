using System.Numerics;

public struct Transform3
{
	public Basis3 Basis;
	public Vector3 Translation;

	public Transform3(Basis3 Basis, Vector3 Translation)
	{
		this.Basis = Basis;
		this.Translation = Translation;
	}

	public readonly Vector3 AppliedTo(Vector3 Vector) => Basis * Vector + Translation;

	public readonly Transform3 AppliedTo(Transform3 Transform) => new Transform3(Basis * Transform.Basis, Translation + Transform.Translation);
}

public struct Basis3
{
	public Vector3 i, j, k;
	
	public static readonly Basis3 Identity = new Basis3(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);

	public Basis3(Vector3 i, Vector3 j, Vector3 k)
	{
		this.i = i;
		this.j = j;
		this.k = k;
	}

	public static Vector3 operator *(Basis3 Basis, Vector3 Vector) => Vector.X * Basis.i + Vector.Y * Basis.j + Vector.Z * Basis.k;

	public static Basis3 operator *(Basis3 First, Basis3 Second) => new Basis3(First * Second.i, First * Second.j, First * Second.k);

	public static Basis3 operator /(Basis3 Basis, float Divisor) => new Basis3(Basis.i / Divisor, Basis.j / Divisor, Basis.k / Divisor);

	public readonly Basis3 Rotated(Vector3 Axis, float Angle)
	{
		return new Basis3(
			i.Rotated(Axis, Angle),
			j.Rotated(Axis, Angle),
			k.Rotated(Axis, Angle)
		);
	}

	public override readonly string ToString() => $"{i}, {j}, {k}";
}