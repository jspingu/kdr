namespace KDR;

public class Model : Spatial
{
    public Mesh Mesh;
    public Material Material;
    
    GeometryBuffer GeometryBuffer;
    GeometryCount GeometryCount;

    public Model(GeometryBuffer geometryBuffer, Mesh mesh, Material material)
    {
        GeometryBuffer = geometryBuffer;
        Mesh = mesh;
        Material = material;

        GeometryCount = mesh.GetCount();
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
        GeometryBuffer.AddGeometry(Mesh, Material, GetGeometryOffset());
    }

    public override void OnTreeExit()
    {
        GeometryBuffer.RemoveGeometry(GetGeometryOffset(), GeometryCount);
    }

    public GeometryCount GetGeometryOffset()
    {
        Entity root = ComposingEntity.Root;
        GeometryCount offset = new();

        if (root is null)
        {
            throw new InvalidOperationException("Model's composing entity does not have a root");
        }

        if(root.GetComponent<Spatial>() is Model m && m.GeometryBuffer == GeometryBuffer)
        {
            if(m == this) return offset;
            offset += m.GeometryCount;
        }

        if(!GetChildrenGeometryOffset(ref offset, root, this))
        {
            throw new InvalidOperationException("Could not find the specified model within the root entity's hierarchy");
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
