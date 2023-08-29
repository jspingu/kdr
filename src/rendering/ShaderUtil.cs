using static SDL2.SDL;
using static System.MathF;
using System.Numerics;

public static class ShaderUtil
{
    public static float WeighBarycentric(Vector3 Weights, float Property1, float Property2, float Property3)
        => Vector3.Dot(Weights, new Vector3(Property1, Property2, Property3));

    public static Vector2 WeighBarycentric(Vector3 Weights, Vector2 Property1, Vector2 Property2, Vector2 Property3)
        => Weights.X * Property1 + Weights.Y * Property2 + Weights.Z * Property3;

    public static Vector3 WeighBarycentric(Vector3 Weights, Vector3 Property1, Vector3 Property2, Vector3 Property3)
        => Weights.X * Property1 + Weights.Y * Property2 + Weights.Z * Property3;

    public static unsafe Vector3 NearestTexel(Vector2 TexCoord, IntPtr Texture)
    {
        SDL_Surface* TexturePtr = (SDL_Surface*)Texture;

        int Unit = Math.Max(TexturePtr->w, TexturePtr->h);

        int TexelX = Math.Clamp((int)(TexCoord.X * Unit), 0, TexturePtr->w - 1);
        int TexelY = Math.Clamp((int)(TexCoord.Y * Unit), 0, TexturePtr->h - 1);

        int Color = ((int*)TexturePtr->pixels)[TexelX + TexelY * TexturePtr->w];
        Vector3 VectorColor = new(Color>>16, (Color>>8) & 255, Color & 255);

        return VectorColor;
    }

    public static int Color(int Red, int Green, int Blue) => (Red<<16) | (Green<<8) | Blue;

    public static int Color(Vector3 Col) => Color((int)Col.X, (int)Col.Y, (int)Col.Z);
}