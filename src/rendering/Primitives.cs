using System.Numerics;

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

public struct IndexedFace
{
    public int i1, i2, i3;
    public Vector3 Normal;

    public IndexedFace(int i1, int i2, int i3, Vector3 Normal)
    {
        this.i1 = i1;
        this.i2 = i2;
        this.i3 = i3;

        this.Normal = Normal;
    }
}

public struct SpatialPrimitive
{
    public Vertex v1, v2, v3;
    public Vector3 Normal;

    public SpatialPrimitive(Vertex v1, Vertex v2, Vertex v3, Vector3 Normal)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        
        this.Normal = Normal;
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