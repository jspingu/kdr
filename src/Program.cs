using KDR;
using static SDL2.SDL;

public static class Program
{
    static readonly int RenderWidth = 960;
    static readonly int RenderHeight = 540;

    public static readonly Rasterizer Rasterizer = new PerspectiveRasterizer(RenderWidth, RenderHeight, 10f, 5000f, MathF.PI / 2f);
    public static readonly Canvas Canvas = new(RenderWidth, RenderHeight);
    public static readonly GeometryBuffer OpaqueGeometryBuffer = new();
    public static readonly GeometryBuffer TransparentGeometryBuffer = new();

    static bool quit = false;

    public static void Quit() => quit = true;

    static void Main()
    {
        SDL_Init(SDL_INIT_VIDEO);

        IntPtr SDLWindow = SDL_CreateWindow(
            "Title",
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            RenderWidth,
            RenderHeight,
            SDL_WindowFlags.SDL_WINDOW_RESIZABLE
        );

        IntPtr SDLRenderer = SDL_CreateRenderer(SDLWindow, -1, SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        
        IntPtr SDLTexture = SDL_CreateTexture(
            SDLRenderer, 
            SDL_PIXELFORMAT_XRGB888, 
            (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, 
            RenderWidth, 
            RenderHeight
        );

        SDL_RenderSetLogicalSize(SDLRenderer, RenderWidth, RenderHeight);

        ulong countOld = SDL_GetPerformanceCounter();

        Entity root = new();
        root
            .SetComponent<Processor>(new RootProcess())
            .SetComponent<Spatial>(new())
            .OnTreeEnter(root);

        while (!quit)
        {
            double delta = (double) (SDL_GetPerformanceCounter() - countOld) / SDL_GetPerformanceFrequency();
            countOld = SDL_GetPerformanceCounter();

            Canvas.Clear();

            root.ProcessCascading((float)delta);
            root.RenderProcessCascading(Transform3.Default);

            Rasterizer.EnableFlags(RasterizerFlags.WriteDepth | RasterizerFlags.CullBackFace);
            Rasterizer.DrawScene(OpaqueGeometryBuffer, Canvas);
            OpaqueGeometryBuffer.ResetState();

            TransparentGeometryBuffer.Sort();
            Rasterizer.DisableFlags(RasterizerFlags.WriteDepth | RasterizerFlags.CullBackFace);
            Rasterizer.DrawScene(TransparentGeometryBuffer, Canvas);
            TransparentGeometryBuffer.ResetState();
            
            Canvas.UploadToSDLTexture(SDLTexture);

            SDL_RenderClear(SDLRenderer);
            SDL_RenderCopy(SDLRenderer, SDLTexture, 0, 0);
            SDL_RenderPresent(SDLRenderer);
        }

        root.OnTreeExit();

        SDL_DestroyWindow(SDLWindow);
        SDL_DestroyRenderer(SDLRenderer);
        SDL_DestroyTexture(SDLTexture);
        
        SDL_Quit();
    }
}
