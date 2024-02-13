namespace KDR;

public struct MaterialBoundFace
{
    public IndexedFace Face;
    public Material Material;

    public MaterialBoundFace(IndexedFace face, Material material)
    {
        Face = face;
        Material = material;
    }
}
