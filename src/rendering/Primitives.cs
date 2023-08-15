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
    public Vector2 Origin, a, b;

    public Primitive(Vector2 Origin, Vector2 a, Vector2 b)
    {
        this.Origin = Origin;
        this.a = a;
        this.b = b;
    }
}