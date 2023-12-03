using System.Numerics;

public struct GeometryCount
{
    public int VertexCount, TextureVertexCount, FaceCount;

    public GeometryCount(int vertexCount, int textureVertexCount, int faceCount)
    {
        VertexCount = vertexCount;
        TextureVertexCount = textureVertexCount;
        FaceCount = faceCount;
    }

    public static GeometryCount operator +(GeometryCount first, GeometryCount second) => new(
        first.VertexCount + second.VertexCount,
        first.TextureVertexCount + second.TextureVertexCount,
        first.FaceCount + second.FaceCount
    );
}

public struct MaterialBoundFace
{
    public IndexedFace Face;
    public Material Material;

    public MaterialBoundFace(IndexedFace face, Material material)
    {
        Face = face;
        Material = material;
    }
}

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

    public IndexedFace OffsetIndices(int vertexOffset, int textureVertexOffset) => new(
        V1 + vertexOffset,
        V2 + vertexOffset,
        V3 + vertexOffset,
        T1 + textureVertexOffset,
        T2 + textureVertexOffset,
        T3 + textureVertexOffset
    );
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