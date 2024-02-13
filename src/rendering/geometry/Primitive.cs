namespace KDR;

public struct Primitive<T> where T : struct
{
    public T V1, V2, V3;

    public Primitive(T v1, T v2, T v3)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }

    public T this[int i]
    {
        get => i switch
        {
            0 => V1,
            1 => V2,
            2 => V3,
            _ => throw new ArgumentOutOfRangeException(nameof(i), "Primitives can only be indexed with 0, 1, and 2")
        };
    }
}
