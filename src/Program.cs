using KDR;
using static SDL2.SDL;
using static SDL2.SDL_image;

public static class Program
{
    static readonly int RenderWidth = 960;
    static readonly int RenderHeight = 540;

    static IProjector MyProjector = new PerspectiveProjector(MathF.PI / 2f);
    static IScanner MyScanner = new PerspectiveScanner(MathF.PI / 2f);

    public static readonly Rasterizer Rasterizer = new Rasterizer(MyProjector, MyScanner, 10f, 5000f);

    public static readonly Canvas Canvas = new(RenderWidth, RenderHeight);
    public static readonly GeometryBuffer OpaqueGeometryBuffer = new();
    public static readonly GeometryBuffer TransparentGeometryBuffer = new();

    static bool quit = false;
    static bool dumpSSVs = false;

    public static void Quit() => quit = true;
    public static void DumpSSVs() => dumpSSVs = true;

    static void Main()
    {
        SDL_Init(SDL_INIT_VIDEO);
        IMG_Init(IMG_InitFlags.IMG_INIT_JPG | IMG_InitFlags.IMG_INIT_PNG);

        IntPtr SDLWindow = SDL_CreateWindow(
            "Title",
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            RenderWidth,
            RenderHeight,
            SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI
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

            if (dumpSSVs)
            {
                foreach (MaterialBoundFace mface in TransparentGeometryBuffer.QueuedFaces)
                {
                    Console.WriteLine(
                        MyProjector.Project(TransparentGeometryBuffer.ViewSpaceVertices[mface.Face.V1], Canvas.Midpoint)
                    );

                    Console.WriteLine(
                        MyProjector.Project(TransparentGeometryBuffer.ViewSpaceVertices[mface.Face.V2], Canvas.Midpoint)
                    );

                    Console.WriteLine(
                        MyProjector.Project(TransparentGeometryBuffer.ViewSpaceVertices[mface.Face.V3], Canvas.Midpoint)
                        + "\n"
                    );
                }

                Quit();
            }

            Rasterizer.DrawScene(OpaqueGeometryBuffer, Canvas, RasterizerFlags.WriteDepth | RasterizerFlags.TestDepth | RasterizerFlags.CullBackFace);
            OpaqueGeometryBuffer.ResetState();

            TransparentGeometryBuffer.Sort();
            Rasterizer.DrawScene(TransparentGeometryBuffer, Canvas, RasterizerFlags.TestDepth | RasterizerFlags.AlphaBlend);
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
        
        IMG_Quit();
        SDL_Quit();
    }
}
