using static SDL2.SDL;
using static System.MathF;

public interface Shader
{
    FragmentData Compute(int FragmentX, int FragmentY);
}

public class Checkerboard : Shader
{
    int TileSize;

    public Checkerboard(int TileSize) => this.TileSize = TileSize;

    public FragmentData Compute(int FragmentX, int FragmentY)
    {
        FragmentX /= TileSize; FragmentY /= TileSize;

        byte intensity = (byte)((FragmentX + FragmentY) % 2 * 255);
        return new FragmentData(new Color(intensity, intensity, intensity), 0);
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
    public FragmentData Compute(int FragmentX, int FragmentY)
    {
        Vector2 PixelCenter = new Vector2(FragmentX + 0.5f, FragmentY + 0.5f);
        Vector2 UV = ShaderUtil.AffineComponent(PixelCenter, Prim);
        Color TexelColor = ShaderUtil.NearestTexel(UV, Texture);

        return new FragmentData(TexelColor, 0);
    }
}

public class RGBTriangle : Shader
{
    public Primitive Prim;

    public RGBTriangle(Primitive Prim) => this.Prim = Prim;

    public FragmentData Compute(int FragmentX, int FragmentY)
    {
        Vector2 PixelCenter = new Vector2(FragmentX + 0.5f, FragmentY + 0.5f);
        Vector2 UV = ShaderUtil.AffineComponent(PixelCenter, Prim);

        byte Red = (byte) ((1 - UV.x - UV.y) * 255);

        Color col = new Color(Red, (byte) (UV.x * 255), (byte) (UV.y * 255));
        return new FragmentData(col, 0);
    }
}

public class ColorFill : Shader
{
    Color FillColor;

    public ColorFill(Color FillColor)
    {
        this.FillColor = FillColor;
    }

    public FragmentData Compute(int FragmentX, int FragmentY)
    {
        return new FragmentData(FillColor, 0);
    }
}

public static class ShaderUtil
{
    public static Vector2 AffineComponent(Vector2 Point, Primitive Basis)
    {
        float AComponent = RayLineIntersect(Point, -Basis.a, Basis.Origin, Basis.b);
        float BComponent = RayLineIntersect(Point, -Basis.b, Basis.Origin, Basis.a); 

        return new Vector2(AComponent, BComponent);

        float RayLineIntersect(Vector2 RayOrigin, Vector2 RayDir, Vector2 LineOrigin, Vector2 LineDir)
        {
            Vector2 LineNormal = new Vector2(-LineDir.y, LineDir.x);
            Vector2 RayDisplacement = RayOrigin - LineOrigin;

            return -RayDisplacement.Dot(LineNormal) / RayDir.Dot(LineNormal);
        }
    }

    public static unsafe Color NearestTexel(Vector2 TexCoord, IntPtr Texture)
    {
        SDL_Surface* TexturePtr = (SDL_Surface*) Texture;

        int TexelX = (int) Floor(Math.Clamp(TexCoord.x, 0, 1) * TexturePtr->w);
        int TexelY = (int) Floor(Math.Clamp(TexCoord.y, 0, 1) * TexturePtr->h);

        uint* Start = (uint*) (TexturePtr->pixels);
        byte* PixelPtr = (byte*) (Start + TexelX + TexelY * TexturePtr->w);

        return new Color(PixelPtr[2], PixelPtr[1], PixelPtr[0]);
    }
}