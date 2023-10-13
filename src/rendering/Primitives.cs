using System.Numerics;

public struct IndexedFace
{
    public int v1, v2, v3;
    public int t1, t2, t3;

    public IndexedFace(int v1, int v2, int v3, int t1, int t2, int t3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;

        this.t1 = t1;
        this.t2 = t2;
        this.t3 = t3;
    }
}

public struct Vertex
{
    public Vector3 Position;
    public Vector2 TexCoord;

    public Vertex(Vector3 Position, Vector2 TexCoord)
    {
        this.Position = Position;
        this.TexCoord = TexCoord;
    }
}

public struct SpatialPrimitive
{
    public Vertex v1, v2, v3;

    public SpatialPrimitive(Vertex v1, Vertex v2, Vertex v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }
}

public struct Primitive
{
    public Vector2 a, b, c;

    public Primitive(Vector2 a, Vector2 b, Vector2 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
}