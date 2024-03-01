namespace KDR;

using System.Numerics;
using static ShaderUtil;

public struct TileMap : IShader
{
    public int Index
    {
        get => _index;

        set
        {
            _index = Math.Clamp(value, 0, HorizontalTiles * VerticalTiles - 1);

            int y = _index / HorizontalTiles;
            int x = _index - y * HorizontalTiles;

            Offset = new(NormalizedTileSize.X * x, NormalizedTileSize.Y * y);
        }
    }

    int _index = 0;

    Texture Texture;
    Vector2 Offset;
    
    int HorizontalTiles, VerticalTiles;
    
    Vector2 NormalizedTileSize;
    float NormalizedTileUnit;

    public TileMap(Texture texture, int horizontalTiles, int verticalTiles)
    {
        Texture = texture;
        HorizontalTiles = horizontalTiles;
        VerticalTiles = verticalTiles;

        NormalizedTileSize = new Vector2((float)Texture.Width / Texture.Unit / horizontalTiles, (float)Texture.Height / Texture.Unit / verticalTiles);
        NormalizedTileUnit = Math.Max(NormalizedTileSize.X, NormalizedTileSize.Y);
    }

    public uint Compute(ShaderParam fragment)
    {
        // fragment.TexCoord should be treated as a coordinate in terms of the tile's coordinate system
        // It must be scaled so that 1 unit is equal to the tile's unit in terms of the tilemap's coordinate system
        Vector2 correctedTexCoord = Offset + fragment.TexCoord * NormalizedTileUnit;

        return 0xFFFFFF | (NearestTexel(correctedTexCoord, Texture) & 0xFF000000);
    }
}
