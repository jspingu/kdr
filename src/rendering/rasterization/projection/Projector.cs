namespace KDR;

using System.Numerics;

public interface IProjector
{
    public Vector2 Project(Vector3 point, Vector2 midpoint);
}
