using static ShaderUtil;
using System.Numerics;

public interface Shader
{
    int Compute(int FragmentX, int FragmentY);
}

public class Checkerboard : Shader
{
    int TileSize;

    public Checkerboard(int TileSize) => this.TileSize = TileSize;

    public int Compute(int FragmentX, int FragmentY)
    {
        FragmentX /= TileSize; FragmentY /= TileSize;

        byte intensity = (byte)((FragmentX + FragmentY) % 2 * 255);
        return Color(intensity, intensity, intensity);
    }
}

public class AffineTextureMap : Shader
{
    IntPtr Texture;
    public Primitive Prim;

    public AffineTextureMap(IntPtr Texture, Primitive Prim)
    {
        this.Texture = Texture;
        this.Prim = Prim;
    }
    public int Compute(int FragmentX, int FragmentY)
    {
        Vector2 PixelCenter = new Vector2(FragmentX + 0.5f, FragmentY + 0.5f);
        Vector2 UV = AffineComponent(PixelCenter, Prim);

        return NearestTexel(UV, Texture);
    }
}

public class RGBTriangle : Shader
{
    public Primitive Prim;

    public RGBTriangle(Primitive Prim) => this.Prim = Prim;

    public int Compute(int FragmentX, int FragmentY)
    {
        Vector2 PixelCenter = new Vector2(FragmentX + 0.5f, FragmentY + 0.5f);
        Vector2 UV = AffineComponent(PixelCenter, Prim);

        byte Red = (byte) ((1 - UV.X - UV.Y) * 255);

        return Color(Red, (byte) (UV.X * 255), (byte) (UV.Y * 255));
    }
}

public class ColorFill : Shader
{
    int FillColor;

    public ColorFill(int FillColor)
    {
        this.FillColor = FillColor;
    }

    public int Compute(int FragmentX, int FragmentY)
    {
        return FillColor;
    }
}