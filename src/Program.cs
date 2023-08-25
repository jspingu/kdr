using static SDL2.SDL;
using System.Numerics;

public static class Program
{
	static readonly int RenderWidth = 960;
	static readonly int RenderHeight = 540;

	public static void Main()
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

		Canvas MyCanvas = new Canvas(RenderWidth, RenderHeight);

		SpatialPrimitive test = new SpatialPrimitive(
			new Vertex(new Vector3(0, -100, 0), Vector2.Zero),
			new Vertex(new Vector3(-100, 100, 0), Vector2.Zero),
			new Vertex(new Vector3(100, 100, 0), Vector2.Zero),
			Vector3.Zero
		);

		Shader Rainbow = new Hello();
		
		SDL_Event e;
		bool quit = false;

		ulong CountOld = SDL_GetPerformanceCounter();
		Queue<double> FrametimeQueue = new Queue<double>();

		while (!quit)
		{
			double TimeDelta = (double) (SDL_GetPerformanceCounter() - CountOld) / SDL_GetPerformanceFrequency();
			CountOld = SDL_GetPerformanceCounter();

			FrametimeQueue.Enqueue(TimeDelta);
			if (FrametimeQueue.Count > 256) FrametimeQueue.Dequeue();

			while (SDL_PollEvent(out e) != 0)
			{
				switch (e.type)
				{
					case SDL_EventType.SDL_QUIT:
						quit = true;
						break;

					case SDL_EventType.SDL_KEYDOWN:
						switch (e.key.keysym.scancode)
						{
							case SDL_Scancode.SDL_SCANCODE_SPACE:
								double total = 0;
								foreach (double Frametime in FrametimeQueue)
								{
									total += Frametime;
								}

								double AvgFrametime = total/256;
								Console.WriteLine($"Avg frametime over 256 frames: {AvgFrametime * 1000}ms ({1/AvgFrametime}FPS)");
								break;
						}

						break;
				}
			}

			MyCanvas.Clear();

			MyCanvas.DrawSpatialPrimitive(test, Rainbow);
			
			MyCanvas.UploadToSDLTexture(SDLTexture);

			SDL_RenderClear(SDLRenderer);
			SDL_RenderCopy(SDLRenderer, SDLTexture, 0, 0);
			SDL_RenderPresent(SDLRenderer);
		}

		SDL_DestroyWindow(SDLWindow);
		SDL_DestroyRenderer(SDLRenderer);
		SDL_DestroyTexture(SDLTexture);
		
		SDL_Quit();
	}
}