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

	public Vector3 AppliedTo(Vector3 Vector) => Basis * Vector + Translation;

	public Transform3 AppliedTo(Transform3 Transform) => new Transform3(Basis * Transform.Basis, Translation + Transform.Translation);
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

	public static Basis3 operator *(Basis3 Basis1, Basis3 Basis2) => new Basis3(Basis1 * Basis2.i, Basis1 * Basis2.j, Basis1 * Basis2.k);

	public Basis3 Rotated(Vector3 Axis, float Angle)
	{
		return new Basis3(
			i.Rotated(Axis, Angle),
			j.Rotated(Axis, Angle),
			k.Rotated(Axis, Angle)
		);
	}

	public override string ToString() => $"{i}, {j}, {k}";
}