namespace KDR;

using System.Numerics;

public static class ShaderUtil
{
    public static uint NearestTexel(Vector2 texCoord, Texture texture)
    {
        int texelX = Math.Clamp((int)(texCoord.X * texture.Unit), 0, texture.Width - 1);
        int texelY = Math.Clamp((int)(texCoord.Y * texture.Unit), 0, texture.Height - 1);

        return texture[texelX + texelY * texture.Width];
    }

    public static uint AlphaDivide(uint d) => d + 1 + (d >> 8) >> 8;

    public static uint AlphaBlend(uint f, uint b)
    {
        uint a = f >> 24;
        uint rb = a * (f & 0xFF00FF) + (255 - a) * (b & 0xFF00FF);
        uint g = a * (f & 0xFF00) + (255 - a) * (b & 0xFF00);

        return rb + 0x10001 + ((rb >> 8) & 0xFF00FF) >> 8 & 0xFF00FF | g + 0x100 + (g >> 8) >> 8 & 0xFF00;
    }

    public static uint Color(uint r, uint g, uint b) => (r<<16) + (g<<8) + b;

    public static uint Color(Vector3 col) => Color((uint)col.X, (uint)col.Y, (uint)col.Z);

    public static Vector3 VectorColor(uint i) => new(i>>16, (i>>8) & 0xFF, i & 0xFF);
}
