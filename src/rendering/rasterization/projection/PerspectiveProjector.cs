namespace KDR;

using System.Numerics;

public class PerspectiveProjector : IProjector
{
    float TanHalfFOV;

    public PerspectiveProjector(float fieldOfView) => TanHalfFOV = MathF.Tan(fieldOfView / 2f);
    
    public Vector2 Project(Vector3 point, Vector2 midpoint) => midpoint + midpoint.X * new Vector2(point.X, -point.Y) / (point.Z * TanHalfFOV);
}
