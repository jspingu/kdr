using System.Numerics;

public class Mesh : Spatial
{
    public Vector3[] Vertices;
    public Vector2[] TextureVertices;
    public IndexedFace[] Faces;

    public Shader Material;

    public Mesh(Vector3[] Vertices, Vector2[] TextureVertices, IndexedFace[] Faces, Shader Material)
    {
        this.Vertices = Vertices;
        this.TextureVertices = TextureVertices;
        this.Faces = Faces;

        this.Material = Material;
    }

    public override void Render(Canvas RenderTarget, Transform3 ViewTransform)
    {
        ViewTransform = ViewTransform.AppliedTo(Transform);

        foreach(IndexedFace Face in Faces)
        {
            SpatialPrimitive ViewTriangle = new(
                new Vertex(ViewTransform.AppliedTo(Vertices[Face.v1]), TextureVertices[Face.t1]),
                new Vertex(ViewTransform.AppliedTo(Vertices[Face.v2]), TextureVertices[Face.t2]),
                new Vertex(ViewTransform.AppliedTo(Vertices[Face.v3]), TextureVertices[Face.t3]),
                
                ViewTransform.Basis * Face.Normal
            );

            RenderTarget.DrawSpatialPrimitive(ViewTriangle, Material);
        }

        foreach(Spatial Child in Children)
        {
            Child.Render(RenderTarget, ViewTransform);
        }
    }
}