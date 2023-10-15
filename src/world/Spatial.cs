using System.Numerics;

public class Spatial
{
    public Transform3 Transform = new(Basis3.Identity, Vector3.Zero);
    public List<Spatial> Children = new();

    public void ProcessCascading(float delta)
    {
        Process(delta);

        foreach(Spatial child in Children)
        {
            child.ProcessCascading(delta);
        }
    }

    public void RenderCascading(Rasterizer rasterizer, Canvas renderTarget, Transform3 viewTransform)
    {
        viewTransform = viewTransform.AppliedTo(Transform);

        Render(rasterizer, renderTarget, viewTransform);

        foreach(Spatial child in Children)
        {
            child.RenderCascading(rasterizer, renderTarget, viewTransform);
        }
    }

    public virtual void Process(float delta) {}

    public virtual void Render(Rasterizer rasterizer, Canvas renderTarget, Transform3 viewTransform) {}
}