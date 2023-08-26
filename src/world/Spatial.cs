using System.Numerics;

public class Spatial
{
    public Transform3 Transform = new(Basis3.Identity, Vector3.Zero);
    public List<Spatial> Children = new();

    public virtual void Render(Canvas RenderTarget, Transform3 ViewTransform)
    {
        foreach(Spatial Child in Children)
        {
            Child.Render(RenderTarget, ViewTransform.AppliedTo(Transform));
        }
    }
}