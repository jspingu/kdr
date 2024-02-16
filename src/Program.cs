using KDR;
using static SDL2.SDL;

using System.Numerics;
using static System.MathF;
using static KDR.MathUtil;

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

    static bool quit = true;
    static bool dumpSSVs = false;

    public static void Quit() => quit = true;
    public static void DumpSSVs() => dumpSSVs = true;

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
        
        SDL_Quit();

        int upperBound = RoundTopLeft(3.3078918f);
        int lowerBound = RoundTopLeft(536.69214f);

        Vector2 start = new(746.69214f, 3.3078918f);
        Vector2 end = new(213.30789f, 536.69214f);

        Scanline[] scanlines = new Scanline[lowerBound - upperBound];
        Scanline[] scanlines2 = new Scanline[lowerBound - upperBound];
        List<(float, int)> offsets = new();
        List<(float, int)> offsets2 = new();

        Trace(start, end, upperBound, scanlines, Canvas, offsets);
        Trace(end, start, upperBound, scanlines2, Canvas, offsets2);

        offsets2.Reverse();

        for (int i = 0; i < offsets.Count; i++)
        {
            Console.WriteLine($"offset: {offsets[i].Item1}, rtplft: {offsets[i].Item2} | offset: {offsets2[i].Item1}, rtplft: {offsets2[i].Item2}");
        }

        void Trace(Vector2 start, Vector2 end, int primUpperBound, Scanline[] scanlines, Canvas renderTarget, List<(float, int)> stuff)
        {
            Vector2 tracePath = end - start;

            if (tracePath.Y == 0) return;

            float slopeX = tracePath.X / tracePath.Y;

            int traceUpperBound = Math.Clamp(RoundTopLeft(Min(start.Y, end.Y)), 0, renderTarget.Height);
            int traceLowerBound = Math.Clamp(RoundTopLeft(Max(start.Y, end.Y)), 0, renderTarget.Height);

            int traceLength = traceLowerBound - traceUpperBound;

            if (tracePath.Y < 0)
            {
                float offsetX = start.X + (traceLowerBound - 0.5f - start.Y) * slopeX;
                int scanlineIndex = traceLowerBound - primUpperBound - 1;

                for (int i = 0; i < traceLength; i++)
                {
                    stuff.Add((offsetX, RoundTopLeft(offsetX)));
                    scanlines[scanlineIndex].LeftBound = Math.Clamp(RoundTopLeft(offsetX), 0, renderTarget.Width);
                    offsetX -= slopeX;
                    scanlineIndex--;
                }
            }
            else
            {
                float offsetX = start.X + (traceUpperBound + 0.5f - start.Y) * slopeX;
                int scanlineIndex = traceUpperBound - primUpperBound;

                for (int i = 0; i < traceLength; i++)
                {
                    stuff.Add((offsetX, RoundTopLeft(offsetX)));
                    scanlines[scanlineIndex].RightBound = Math.Clamp(RoundTopLeft(offsetX), 0, renderTarget.Width);
                    offsetX += slopeX;
                    scanlineIndex++;
                }
            }
        }
    }
}
