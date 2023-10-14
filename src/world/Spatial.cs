using System.Numerics;

public class Spatial
{
    public Transform3 Transform = new(Basis3.Identity, Vector3.Zero);
    public List<Spatial> Children = new();

    public virtual void Render(Rasterizer rasterizer, Canvas renderTarget, Transform3 viewTransform)
    {
        foreach(Spatial child in Children)
        {
            child.Render(rasterizer, renderTarget, viewTransform.AppliedTo(Transform));
        }
    }
}