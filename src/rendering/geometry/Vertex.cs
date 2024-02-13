namespace KDR;

using System.Numerics;

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
