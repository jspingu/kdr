using static SDL2.SDL;
using static System.MathF;
using System.Numerics;

public static class ShaderUtil
{
    public static unsafe int NearestTexel(Vector2 texCoord, IntPtr texture)
    {
        SDL_Surface* texturePtr = (SDL_Surface*)texture;

        int unit = Math.Max(texturePtr->w, texturePtr->h);

        int texelX = Math.Clamp((int)(texCoord.X * unit), 0, texturePtr->w - 1);
        int texelY = Math.Clamp((int)(texCoord.Y * unit), 0, texturePtr->h - 1);

        return ((int*)texturePtr->pixels)[texelX + texelY * texturePtr->w];
    }

    public static int Color(int r, int g, int b) => (r<<16) + (g<<8) + b;

    public static int Color(Vector3 col) => Color((int)col.X, (int)col.Y, (int)col.Z);

    public static Vector3 VectorColor(int i) => new(i>>16, (i>>8) & 0xFF, i & 0xFF);
}