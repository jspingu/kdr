using static SDL2.SDL;
using static System.MathF;
using System.Numerics;

public static class ShaderUtil
{
    public static Vector2 AffineComponent(Vector2 Point, Primitive Basis)
    {
        float AComponent = RayLineIntersect(Point, -Basis.a, Basis.Origin, Basis.b);
        float BComponent = RayLineIntersect(Point, -Basis.b, Basis.Origin, Basis.a); 

        return new Vector2(AComponent, BComponent);

        float RayLineIntersect(Vector2 RayOrigin, Vector2 RayDir, Vector2 LineOrigin, Vector2 LineDir)
        {
            Vector2 LineNormal = new Vector2(-LineDir.Y, LineDir.X);
            Vector2 RayDisplacement = RayOrigin - LineOrigin;

            return Vector2.Dot(-RayDisplacement, LineNormal) / Vector2.Dot(RayDir, LineNormal);
        }
    }

    public static unsafe int NearestTexel(Vector2 TexCoord, IntPtr Texture)
    {
        SDL_Surface* TexturePtr = (SDL_Surface*)Texture;

        int TexelX = (int) Floor(Math.Clamp(TexCoord.X, 0, 1) * TexturePtr->w);
        int TexelY = (int) Floor(Math.Clamp(TexCoord.Y, 0, 1) * TexturePtr->h);

        return ((int*)TexturePtr->pixels)[TexelX + TexelY * TexturePtr->w];
    }

    public static int Color(int Red, int Green, int Blue) => (Red<<16) | (Green<<8) | Blue;
}