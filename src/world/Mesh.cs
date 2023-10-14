using System.Numerics;

public class Mesh
{
    public readonly Vector3[] Vertices;
    public readonly Vector2[] TextureVertices;
    public readonly IndexedFace[] Faces;

    public Mesh(Vector3[] vertices, Vector2[] textureVertices, IndexedFace[] faces)
    {
        Vertices = vertices;
        TextureVertices = textureVertices;
        Faces = faces;
    }
}