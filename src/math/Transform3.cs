using System.Numerics;

public struct Transform3
{
	public Basis3 Basis;
	public Vector3 Translation;

	public static readonly Transform3 Default = new Transform3(Basis3.Identity, Vector3.Zero);

	public Transform3(Basis3 basis, Vector3 translation)
	{
		Basis = basis;
		Translation = translation;
	}

	public readonly Vector3 AppliedTo(Vector3 vector) => Basis * vector + Translation;

	public readonly Transform3 AppliedTo(Transform3 transform) => new Transform3(Basis * transform.Basis, this.AppliedTo(transform.Translation));
}

public struct Basis3
{
	public Vector3 I, J, K;
	
	public static readonly Basis3 Identity = new Basis3(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);

	public Basis3(Vector3 i, Vector3 j, Vector3 k)
	{
		I = i;
		J = j;
		K = k;
	}

	public static Vector3 operator *(Basis3 basis, Vector3 vector) => vector.X * basis.I + vector.Y * basis.J + vector.Z * basis.K;

	public static Basis3 operator *(Basis3 first, Basis3 second) => new Basis3(first * second.I, first * second.J, first * second.K);

	public static Basis3 operator /(Basis3 basis, float divisor) => new Basis3(basis.I / divisor, basis.J / divisor, basis.K / divisor);

	public readonly Basis3 Rotated(Vector3 axis, float angle)
	{
		return new Basis3(
			I.Rotated(axis, angle),
			J.Rotated(axis, angle),
			K.Rotated(axis, angle)
		);
	}

	public void Rotate(Vector3 axis, float angle)
	{
		I = I.Rotated(axis, angle);
		J = J.Rotated(axis, angle);
		K = K.Rotated(axis, angle);
	}

	public override readonly string ToString() => $"{I}, {J}, {K}";
}