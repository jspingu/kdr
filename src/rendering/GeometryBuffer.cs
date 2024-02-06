namespace KDR;

using System.Numerics;

public class GeometryBuffer
{
    List<Vector3> Vertices = new();
    List<MaterialBoundFace> UnqueuedFaces = new();
    int TraversedVertices, TraversedFaces;
    
    public Vector3[] ViewSpaceVertices = Array.Empty<Vector3>();
    public List<Vector2> TextureVertices = new();
    public List<MaterialBoundFace> QueuedFaces = new();

    public void AddGeometry(Mesh mesh, Material material, GeometryCount offset)
    {
        GeometryCount meshCount = mesh.GetCount();

        Vertices.InsertRange(offset.VertexCount, mesh.Vertices);
        TextureVertices.InsertRange(offset.TextureVertexCount, mesh.TextureVertices);
        
        UnqueuedFaces.InsertRange(offset.FaceCount, Array.ConvertAll(mesh.Faces, (face) => new MaterialBoundFace(
            face.OffsetIndices(offset.VertexCount, offset.TextureVertexCount), 
            material
        )));
        
        for(int i = offset.FaceCount + meshCount.FaceCount; i < UnqueuedFaces.Count; i++)
        {
            UnqueuedFaces[i] = new MaterialBoundFace(
                UnqueuedFaces[i].Face.OffsetIndices(meshCount.VertexCount, meshCount.TextureVertexCount),
                UnqueuedFaces[i].Material
            );
        }

        Array.Resize(ref ViewSpaceVertices, Vertices.Capacity);
    }

    public void RemoveGeometry(GeometryCount offset, GeometryCount count)
    {
        Vertices.RemoveRange(offset.VertexCount, count.VertexCount);
        TextureVertices.RemoveRange(offset.TextureVertexCount, count.TextureVertexCount);
        UnqueuedFaces.RemoveRange(offset.FaceCount, count.FaceCount);

        for(int i = offset.FaceCount; i < UnqueuedFaces.Count; i++)
        {
            UnqueuedFaces[i] = new MaterialBoundFace(
                UnqueuedFaces[i].Face.OffsetIndices(-count.VertexCount, -count.TextureVertexCount),
                UnqueuedFaces[i].Material
            );
        }
    }

    public void SetMaterialBinding(Material material, int faceOffset, int faceCount)
    {
        for(int i = faceOffset; i < faceOffset + faceCount; i++)
        {
            UnqueuedFaces[i] = new MaterialBoundFace(UnqueuedFaces[i].Face, material);
        }
    }

    public void TraverseVertices(int vertexCount) => TraversedVertices += vertexCount;

    public void TransformVertices(int vertexCount, Transform3 viewTransform)
    {
        for(int i = TraversedVertices; i < TraversedVertices + vertexCount; i++)
        {
            ViewSpaceVertices[i] = viewTransform.AppliedTo(Vertices[i]);
        }

        TraversedVertices += vertexCount;
    }

    public void TraverseFaces(int faceCount) => TraversedFaces += faceCount;

    public void QueueFaces(int faceCount)
    {
        QueuedFaces.AddRange(UnqueuedFaces.GetRange(TraversedFaces, faceCount));
        TraversedFaces += faceCount;
    }

    public void ResetState()
    {
        TraversedVertices = 0;
        TraversedFaces = 0;

        QueuedFaces.Clear();
    }

    // public void Sort()
    // {
    //     QueuedFaces.Sort((matface1, matface2) => {
    //         Vector3 n1 = Vector3.Cross(
    //             ViewSpaceVertices[matface1.Face.V2] - ViewSpaceVertices[matface1.Face.V1],
    //             ViewSpaceVertices[matface1.Face.V3] - ViewSpaceVertices[matface1.Face.V1]
    //         );

    //         Vector3 midpoint1 = (ViewSpaceVertices[matface1.Face.V1] + ViewSpaceVertices[matface1.Face.V2] + ViewSpaceVertices[matface1.Face.V3]) / 3;
    //         Vector3 midpoint2 = (ViewSpaceVertices[matface2.Face.V1] + ViewSpaceVertices[matface2.Face.V2] + ViewSpaceVertices[matface2.Face.V3]) / 3;

    //         return (int)(midpoint1 - midpoint2).Z;
    //     });
    // }
}
