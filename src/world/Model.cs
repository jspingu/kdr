public class Model<TShader> : Spatial where TShader : struct, IShader
{
    public Mesh Mesh;
    public Material<TShader> Material;

    public Model(Mesh Mesh, Material<TShader> Material)
    {
        this.Mesh = Mesh;
        this.Material = Material;
    }

    public override void Render(Canvas RenderTarget, Transform3 ViewTransform)
    {
        ViewTransform = ViewTransform.AppliedTo(Transform);

        foreach(IndexedFace Face in Mesh.Faces)
        {
            SpatialPrimitive ViewTriangle = new(
                new Vertex(ViewTransform.AppliedTo(Mesh.Vertices[Face.v1]), Mesh.TextureVertices[Face.t1]),
                new Vertex(ViewTransform.AppliedTo(Mesh.Vertices[Face.v2]), Mesh.TextureVertices[Face.t2]),
                new Vertex(ViewTransform.AppliedTo(Mesh.Vertices[Face.v3]), Mesh.TextureVertices[Face.t3]),
                
                ViewTransform.Basis * Face.Normal
            );

            RenderTarget.DrawSpatialPrimitive(ViewTriangle, Material.Shader);
        }

        foreach(Spatial Child in Children)
        {
            Child.Render(RenderTarget, ViewTransform);
        }
    }
}