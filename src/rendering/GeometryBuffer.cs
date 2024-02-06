namespace KDR;

using System.Numerics;

public class GeometryBuffer
{
    List<Vector3> Vertices = new();
    int TraversedVertices, TraversedFaces;
    
    public List<Vector2> TextureVertices = new();
    public List<MaterialBoundFace> MatFaces = new();
    public List<MaterialBoundFace> QueuedFaces = new();

    public Vector3[] ViewSpaceVertices = Array.Empty<Vector3>();

    public GeometryCount AddModel(Model model, GeometryCount offset)
    {
        Mesh mesh = model.Mesh;

        GeometryCount meshCount = new(
            mesh.Vertices.Length,
            mesh.TextureVertices.Length,
            mesh.Faces.Length
        );

        Vertices.InsertRange(offset.VertexCount, mesh.Vertices);
        TextureVertices.InsertRange(offset.TextureVertexCount, mesh.TextureVertices);
        
        MatFaces.InsertRange(offset.FaceCount, Array.ConvertAll(mesh.Faces, (face) => new MaterialBoundFace(
            face.OffsetIndices(offset.VertexCount, offset.TextureVertexCount), 
            model.Material
        )));
        
        for(int i = offset.FaceCount + meshCount.FaceCount; i < MatFaces.Count; i++)
        {
            MatFaces[i] = new MaterialBoundFace(
                MatFaces[i].Face.OffsetIndices(meshCount.VertexCount, meshCount.TextureVertexCount),
                MatFaces[i].Material
            );
        }

        Array.Resize(ref ViewSpaceVertices, Vertices.Capacity);

        return meshCount;
    }

    public void RemoveModel(GeometryCount offset, GeometryCount count)
    {
        Vertices.RemoveRange(offset.VertexCount, count.VertexCount);
        TextureVertices.RemoveRange(offset.TextureVertexCount, count.TextureVertexCount);
        MatFaces.RemoveRange(offset.FaceCount, count.FaceCount);

        for(int i = offset.FaceCount; i < MatFaces.Count; i++)
        {
            MatFaces[i] = new MaterialBoundFace(
                MatFaces[i].Face.OffsetIndices(-count.VertexCount, -count.TextureVertexCount),
                MatFaces[i].Material
            );
        }
    }

    public void SetMaterialBinding(Material material, int faceOffset, int faceCount)
    {
        for(int i = faceOffset; i < faceOffset + faceCount; i++)
        {
            MatFaces[i] = new MaterialBoundFace(MatFaces[i].Face, material);
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
        QueuedFaces.AddRange(MatFaces.GetRange(TraversedFaces, faceCount));
        TraversedFaces += faceCount;
    }

    public void ResetState()
    {
        TraversedVertices = 0;
        TraversedFaces = 0;

        QueuedFaces.Clear();
    }

    public void Sort()
    {
        QueuedFaces.Sort((matFace1, matFace2) => {
            Primitive<Vector3> prim1 = new(
                ViewSpaceVertices[matFace1.Face.V1],
                ViewSpaceVertices[matFace1.Face.V2],
                ViewSpaceVertices[matFace1.Face.V3]
            );

            Primitive<Vector3> prim2 = new(
                ViewSpaceVertices[matFace2.Face.V1],
                ViewSpaceVertices[matFace2.Face.V2],
                ViewSpaceVertices[matFace2.Face.V3]
            );

            return 0;
        });

        Vector3 GetTriangleNormal(Primitive<Vector3> triangle)
        {
            Vector3 normal = Vector3.Cross(triangle.V2 - triangle.V1, triangle.V3 - triangle.V1);
            return Vector3.Dot(triangle.V1, normal) > 0 ? -normal : normal;
        }

        int TriangleOneSideThing(Primitive<Vector3> triangle, Vector3 normal)
        {
            int flagPositive = 0;
            int flagNegative = 0;

            for (int i = 0; i < 3; i++)
            {
                if (Vector3.Dot(triangle[i], normal) > 0) flagPositive = 1;
                else if (Vector3.Dot(triangle[i], normal) < 0) flagNegative = -1;
            }

            return flagNegative + flagPositive;
        }

        // Further away triangles are "less than" closer triangles
    }
}
