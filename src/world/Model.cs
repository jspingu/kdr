using System.Numerics;

public class Model<TShader> : Spatial where TShader : struct, IShader
{
    public Mesh Mesh;
    public Material<TShader> Material;

    public Model(Mesh mesh, Material<TShader> material)
    {
        Mesh = mesh;
        Material = material;
    }

    public override void Render(Rasterizer rasterizer, Canvas renderTarget, Transform3 viewTransform)
    {
        Vector3[] viewSpaceVertices = new Vector3[Mesh.Vertices.Length];
        for(int i = 0; i < viewSpaceVertices.Length; i++) viewSpaceVertices[i] = viewTransform.AppliedTo(Mesh.Vertices[i]);

        rasterizer.DrawModel(viewSpaceVertices, Mesh.TextureVertices, Mesh.Faces, renderTarget, Material.Shader);
    }
}