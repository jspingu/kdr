namespace KDR;

using System.Numerics;
using static ShaderUtil;

public struct TileMap : IShader
{
    public int X, Y;

    Texture Texture;
    Vector2 NormalizedTileSize;

    public TileMap(Texture texture, int horizontalTiles, int verticalTiles)
    {
        Texture = texture;
        NormalizedTileSize = new Vector2(Texture.Width / (Texture.Unit * horizontalTiles), Texture.Height / (Texture.Unit * verticalTiles));
    }

    public uint Compute(ShaderParam fragment)
    {
        Vector2 offset = new(NormalizedTileSize.X * X, NormalizedTileSize.Y * Y);
        Vector2 correctedTexCoord = offset + fragment.TexCoord * NormalizedTileSize;

        return NearestTexel(correctedTexCoord, Texture);
    }
}
