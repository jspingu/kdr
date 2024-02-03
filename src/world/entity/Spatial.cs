namespace KDR;

using System.Numerics;

public class Spatial : EntityComponent
{
    public Transform3 Transform = new(Basis3.Identity, Vector3.Zero);

    public virtual Transform3 RenderProcess(Transform3 viewTransform) => viewTransform.AppliedTo(Transform);
}
