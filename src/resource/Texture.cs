namespace KDR;

using static SDL2.SDL;
using static SDL2.SDL_image;

public unsafe class Texture : IDisposable
{
    public int Width;
    public int Height;
    public int Unit;

    SDL_Surface* Surface;

    public uint this[int i]
    {
        get => ((uint*)Surface->pixels)[i];
    }

    public Texture(string path)
    {
        Surface = (SDL_Surface*)IMG_Load(path);
        if (Surface == null) throw new FileNotFoundException("Failed to load texture", path);

        bool hasAlpha = SDL_ISPIXELFORMAT_ALPHA(((SDL_PixelFormat*)Surface->format)->format);

        if ( ((SDL_PixelFormat*)Surface->format)->format != SDL_PIXELFORMAT_ABGR8888 )
        {
            SDL_Surface* temp = (SDL_Surface*)SDL_ConvertSurfaceFormat((IntPtr)Surface, SDL_PIXELFORMAT_ABGR8888, 0);
            SDL_FreeSurface((IntPtr)Surface);
            Surface = temp;

            if (!hasAlpha)
            {
                SDL_LockSurface((IntPtr)Surface);
                
                uint* pixel = (uint*)(Surface->pixels);
                for (int i = 0; i < Surface->w * Surface->h; i++) *pixel++ |= 0xFF000000;

                SDL_UnlockSurface((IntPtr)Surface);
            }
        }

        Width = Surface->w;
        Height = Surface->h;
        Unit = Math.Max(Surface->w, Surface->h);   
    }

    public void Dispose()
    {
        SDL_FreeSurface((IntPtr)Surface);
    }
}
