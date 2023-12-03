public class Model : Spatial
{
    public GeometryBuffer GeometryBuffer;
    public GeometryCount GeometryCount;

    public Mesh Mesh;
    public Material Material;

    public Model(GeometryBuffer geometryBuffer, Mesh mesh, Material material)
    {
        GeometryBuffer = geometryBuffer;
        Mesh = mesh;
        Material = material;
    }

    public override Transform3 RenderProcess(Transform3 viewTransform)
    {
        viewTransform = base.RenderProcess(viewTransform);
        GeometryBuffer.TransformVertices(GeometryCount.VertexCount, viewTransform);
        GeometryBuffer.QueueFaces(GeometryCount.FaceCount);

        return viewTransform;
    }

    public override void OnTreeEnter()
    {
        GeometryCount = GeometryBuffer.AddModel(this, GetGeometryOffset(ThisEntity.Root, this));
    }

    public override void OnTreeExit()
    {
        GeometryBuffer.RemoveModel(GetGeometryOffset(ThisEntity.Root, this), GeometryCount);
    }

    public static GeometryCount GetGeometryOffset(Entity root, Model model)
    {
        GeometryCount offset = new();

        if(root.GetComponent<Spatial>() is Model m && m.GeometryBuffer == model.GeometryBuffer)
        {
            if(m == model) return offset;
            offset += m.GeometryCount;
        }

        if(!GetChildrenGeometryOffset(ref offset, root, model))
        {
            throw new InvalidOperationException("Could not find the specified model within the specified root entity's hierarchy");
        }

        return offset;

        static bool GetChildrenGeometryOffset(ref GeometryCount offset, Entity entity, Model model)
        {
            foreach(Entity child in entity.Children)
            {
                if(child.GetComponent<Spatial>() is Model m && m.GeometryBuffer == model.GeometryBuffer)
                {
                    if(m == model) return true;
                    offset += m.GeometryCount;
                }

                if(GetChildrenGeometryOffset(ref offset, child, model)) return true;
            }

            return false;
        }
    }
}