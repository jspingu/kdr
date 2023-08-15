using System.Numerics;

public class Spatial
{
    public Transform3 Transform = new Transform3(Basis3.Identity, Vector3.Zero);
    public List<Spatial> Children = new List<Spatial>();

    public virtual void Render(Canvas RenderTarget, Transform3 ViewTransform)
    {
        foreach(Spatial Child in Children)
        {
            Child.Render(RenderTarget, ViewTransform.AppliedTo(Transform));
        }
    }
}