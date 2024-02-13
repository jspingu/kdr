namespace KDR;

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
