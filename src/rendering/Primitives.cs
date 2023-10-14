using System.Numerics;

public struct IndexedFace
{
    public int V1, V2, V3;
    public int T1, T2, T3;

    public IndexedFace(int v1, int v2, int v3, int t1, int t2, int t3)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        
        T1 = t1;
        T2 = t2;
        T3 = t3;
    }
}

public struct Vertex
{
    public Vector3 Position;
    public Vector2 TexCoord;

    public Vertex(Vector3 position, Vector2 texCoord)
    {
        Position = position;
        TexCoord = texCoord;
    }
}

public struct SpatialPrimitive
{
    public Vertex V1, V2, V3;

    public SpatialPrimitive(Vertex v1, Vertex v2, Vertex v3)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }
}

public struct Primitive
{
    public Vector2 A, B, C;

    public Primitive(Vector2 a, Vector2 b, Vector2 c)
    {
        A = a;
        B = b;
        C = c;
    }
}