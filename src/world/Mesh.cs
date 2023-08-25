public class Mesh : Spatial
{
    Vertex[] Vertices;
    IndexedFace[] Faces;

    public Mesh(Vertex[] Vertices, IndexedFace[] Faces)
    {
        this.Vertices = Vertices;
        this.Faces = Faces;
    }

    public override void Render(Canvas RenderTarget, Transform3 ViewTransform)
    {
        base.Render(RenderTarget, ViewTransform);
    }
}