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
        QueuedFaces.Sort((faceA, faceB) => {
            Primitive<Vector3> primA = new(
                ViewSpaceVertices[faceA.Face.V1],
                ViewSpaceVertices[faceA.Face.V2],
                ViewSpaceVertices[faceA.Face.V3]
            );

            Primitive<Vector3> primB = new(
                ViewSpaceVertices[faceB.Face.V1],
                ViewSpaceVertices[faceB.Face.V2],
                ViewSpaceVertices[faceB.Face.V3]
            );

            // Less than 0: faceA < faceB (faceA is behind faceB)
            // Greater than 0: faceA > faceB (faceA is in front of faceB)
            
            int ADisplacementFromB = TriangleDisplacement(primA, primB);
            if (ADisplacementFromB != 0) return ADisplacementFromB;            

            int BDisplacementFromA = TriangleDisplacement(primB, primA);
            if (BDisplacementFromA != 0) return -BDisplacementFromA;            

            // Neither face A nor face B are entirely contained on one side of the other
            // Sort by midpoint z values

            float midpointZA = (primA.V1.Z + primA.V2.Z + primA.V3.Z) / 3;
            float midpointZB = (primB.V1.Z + primB.V2.Z + primB.V3.Z) / 3;
            
            return (int)(midpointZB - midpointZA);
        });

        int TriangleDisplacement(Primitive<Vector3> displaced, Primitive<Vector3> from)
        {
            Vector3 normal = Vector3.Cross(from.V2 - from.V1, from.V3 - from.V1);
            float normalDisplacement = Vector3.Dot(from.V1, normal);

            normal = normalDisplacement > 0 ? -normal : normal;

            int flagPositive = 0;
            int flagNegative = 0;

            for (int i = 0; i < 3; i++)
            {
                if (Vector3.Dot(displaced[i], normal) > normalDisplacement) flagPositive = 1;
                else if (Vector3.Dot(displaced[i], normal) < normalDisplacement) flagNegative = -1;
            }

            return flagNegative + flagPositive;
        }
    }
}
