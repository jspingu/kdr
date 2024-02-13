namespace KDR;

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
