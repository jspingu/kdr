using System.Numerics;

public struct Transform2
{
    public Basis2 Basis;
    public Vector2 Translation;

    public Transform2(Basis2 basis, Vector2 translation)
    {
        Basis = basis;
        Translation = translation;
    }

    public readonly Vector2 AppliedTo(Vector2 vector) => Basis * vector + Translation;

	public readonly Transform2 AppliedTo(Transform2 transform) => new Transform2(Basis * transform.Basis, Translation + transform.Translation);
}

public struct Basis2
{
    public Vector2 I, J;

    public static readonly Basis2 Identity = new Basis2(Vector2.UnitX, Vector2.UnitY);

    public Basis2(Vector2 i, Vector2 j)
    {
        I = i;
        J = j;
    }

    public static Vector2 operator *(Basis2 basis, Vector2 vector) => vector.X * basis.I + vector.Y * basis.J;

    public static Vector2 operator *(Vector2 vector, Basis2 basis) => new Vector2(Vector2.Dot(basis.I, vector), Vector2.Dot(basis.J, vector));

    public static Basis2 operator *(Basis2 first, Basis2 second) => new Basis2(first * second.I, first * second.J);

    public static Basis2 operator /(Basis2 basis, float divisor) => new Basis2(basis.I / divisor, basis.J / divisor);

    public readonly Basis2 Inversed() => new Basis2(
        new Vector2(J.Y, -I.Y), 
        new Vector2(-J.X, I.X)
    ) / Vector2.Dot(I, J.CounterClockwiseNormal());

    public override readonly string ToString() => $"{I}, {J}";
}