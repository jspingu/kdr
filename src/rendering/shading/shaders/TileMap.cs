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
            if (value < 0 || value >= HorizontalTiles * VerticalTiles) throw new IndexOutOfRangeException($"Tilemap index {value} out of range");
            _index = value;

            int y = _index / HorizontalTiles;
            int x = _index - y * HorizontalTiles;

            Console.WriteLine($"X: {x}, Y: {y}");

            Offset = new(NormalizedTileSize.X * x, NormalizedTileSize.Y * y);
            Console.WriteLine(Offset);
        }
    }

    int _index = 0;

    int HorizontalTiles, VerticalTiles;

    Texture Texture;
    Vector2 NormalizedTileSize;
    Vector2 Offset;
    
    float TileUnit;

    public TileMap(Texture texture, int horizontalTiles, int verticalTiles)
    {
        Texture = texture;
        HorizontalTiles = horizontalTiles;
        VerticalTiles = verticalTiles;

        NormalizedTileSize = new Vector2((float)Texture.Width / (Texture.Unit * horizontalTiles), (float)Texture.Height / (Texture.Unit * verticalTiles));
        TileUnit = Math.Max(NormalizedTileSize.X, NormalizedTileSize.Y);
    }

    public uint Compute(ShaderParam fragment)
    {
        Vector2 correctedTexCoord = Offset + fragment.TexCoord * TileUnit;

        return 0xFFFFFF | (NearestTexel(correctedTexCoord, Texture) & 0xFF000000);
    }
}
