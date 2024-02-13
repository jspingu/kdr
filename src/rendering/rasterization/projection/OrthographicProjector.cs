namespace KDR;

using System.Numerics;

public class OrthographicProjector : IProjector
{
    public Vector2 Project(Vector3 point, Vector2 midpoint) => midpoint + new Vector2(point.X, -point.Y);
}

