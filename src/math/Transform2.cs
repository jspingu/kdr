using System.Numerics;

public struct Transform2
{
    public Basis2 Basis;
    public Vector2 Translation;

    public Transform2(Basis2 Basis, Vector2 Translation)
    {
        this.Basis = Basis;
        this.Translation = Translation;
    }

    public readonly Vector2 AppliedTo(Vector2 Vector) => Basis * Vector + Translation;

	public readonly Transform2 AppliedTo(Transform2 Transform) => new Transform2(Basis * Transform.Basis, Translation + Transform.Translation);
}

public struct Basis2
{
    public Vector2 i, j;

    public static readonly Basis2 Identity = new Basis2(Vector2.UnitX, Vector2.UnitY);

    public Basis2(Vector2 i, Vector2 j)
    {
        this.i = i;
        this.j = j;
    }

    public static Vector2 operator *(Basis2 Basis, Vector2 Vector) => Vector.X * Basis.i + Vector.Y * Basis.j;

    public static Vector2 operator *(Vector2 Vector, Basis2 Basis) => new Vector2(Vector2.Dot(Basis.i, Vector), Vector2.Dot(Basis.j, Vector));

    public static Basis2 operator *(Basis2 First, Basis2 Second) => new Basis2(First * Second.i, First * Second.j);

    public static Basis2 operator /(Basis2 Basis, float Divisor) => new Basis2(Basis.i / Divisor, Basis.j / Divisor);

    public readonly Basis2 Inversed() => new Basis2(
        new Vector2(j.Y, -i.Y), 
        new Vector2(-j.X, i.X)
    ) / Vector2.Dot(i, j.CounterClockwiseNormal());

    public override readonly string ToString() => $"{i}, {j}";
}