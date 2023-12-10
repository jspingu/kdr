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

    public static Mesh CreateRectangleMesh(int width, int height)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        return new Mesh(
            new Vector3[] {new(-halfWidth, halfHeight, 0), new(halfWidth, halfHeight, 0), new(-halfWidth, -halfHeight, 0), new(halfWidth, -halfHeight, 0)},
            new Vector2[] {new(0, 0), new(1, 0), new(0, 1), new(1, 1)},
            new IndexedFace[] {new(0, 1, 2, 0, 1, 2), new(3, 2, 1, 3, 2, 1)}
        );
    }
}