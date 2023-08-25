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
}

public struct Basis2
{
    public Vector2 i, j;

    public Basis2(Vector2 i, Vector2 j)
    {
        this.i = i;
        this.j = j;
    }

    public static Vector2 operator *(Basis2 Basis, Vector2 Vector) => Vector.X * Basis.i + Vector.Y * Basis.j;
}