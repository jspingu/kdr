using System.Numerics;

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

        Vector3[] ViewSpaceVertices = new Vector3[Mesh.Vertices.Length];

        for(int i = 0; i < ViewSpaceVertices.Length; i++) ViewSpaceVertices[i] = ViewTransform.AppliedTo(Mesh.Vertices[i]);

        foreach(IndexedFace Face in Mesh.Faces)
        {
            SpatialPrimitive ViewTriangle = new(
                new Vertex(ViewSpaceVertices[Face.v1], Mesh.TextureVertices[Face.t1]),
                new Vertex(ViewSpaceVertices[Face.v2], Mesh.TextureVertices[Face.t2]),
                new Vertex(ViewSpaceVertices[Face.v3], Mesh.TextureVertices[Face.t3])
            );

            RenderTarget.DrawSpatialPrimitive(ViewTriangle, Material.Shader);
        }

        foreach(Spatial Child in Children)
        {
            Child.Render(RenderTarget, ViewTransform);
        }
    }
}