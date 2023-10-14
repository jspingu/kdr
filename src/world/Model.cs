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

    public override void Render(Rasterizer Rasterizer, Canvas RenderTarget, Transform3 ViewTransform)
    {
        ViewTransform = ViewTransform.AppliedTo(Transform);

        Vector3[] ViewSpaceVertices = new Vector3[Mesh.Vertices.Length];

        for(int i = 0; i < ViewSpaceVertices.Length; i++) ViewSpaceVertices[i] = ViewTransform.AppliedTo(Mesh.Vertices[i]);

        Rasterizer.DrawModel(ViewSpaceVertices, Mesh.TextureVertices, Mesh.Faces, RenderTarget, Material.Shader);

        foreach(Spatial Child in Children)
        {
            Child.Render(Rasterizer, RenderTarget, ViewTransform);
        }
    }
}