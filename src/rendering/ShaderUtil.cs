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

    public static unsafe int NearestTexel(Vector2 TexCoord, IntPtr Texture)
    {
        SDL_Surface* TexturePtr = (SDL_Surface*)Texture;

        int TexelX = (int) Floor(Math.Clamp(TexCoord.X, 0, 1) * TexturePtr->w);
        int TexelY = (int) Floor(Math.Clamp(TexCoord.Y, 0, 1) * TexturePtr->h);

        return ((int*)TexturePtr->pixels)[TexelX + TexelY * TexturePtr->w];
    }

    public static int Color(int Red, int Green, int Blue) => (Red<<16) | (Green<<8) | Blue;
}