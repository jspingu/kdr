using System.Numerics;

public class Mesh
{
    public readonly Vector3[] Vertices;
    public readonly Vector2[] TextureVertices;
    public readonly IndexedFace[] Faces;

    public Mesh(Vector3[] Vertices, Vector2[] TextureVertices, IndexedFace[] Faces)
    {
        this.Vertices = Vertices;
        this.TextureVertices = TextureVertices;
        this.Faces = Faces;
    }
}