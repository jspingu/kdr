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

		ulong countOld = SDL_GetPerformanceCounter();
		Queue<double> frametimeQueue = new();

		bool mouseCaptured = false;
        bool quit = false;

		Rasterizer myRasterizer = new PerspectiveRasterizer(RenderWidth, RenderHeight, 50f, MathF.PI / 2f);
		Canvas myCanvas = new(RenderWidth, RenderHeight);
		
		TextureMap cubeTexture = new(SDL_LoadBMP("images/wood.bmp"));
		Model<TextureMap> cube = new(MeshBuilder.BuildFromFile("assets/cube.mesh"), new(cubeTexture));

		float distance = 600;

        while (!quit)
		{
			double timeDelta = (double) (SDL_GetPerformanceCounter() - countOld) / SDL_GetPerformanceFrequency();
			countOld = SDL_GetPerformanceCounter();

			frametimeQueue.Enqueue(timeDelta);
			if (frametimeQueue.Count > 256) frametimeQueue.Dequeue();

			while (SDL_PollEvent(out SDL_Event e) != 0)
			{
				switch (e.type)
				{
					case SDL_EventType.SDL_QUIT:
						quit = true;
						break;

					case SDL_EventType.SDL_MOUSEMOTION:
						if (!mouseCaptured) break;

						cube.Transform.Basis = cube.Transform.Basis.Rotated(Vector3.UnitY, -e.motion.xrel * 0.005f);
						cube.Transform.Basis = cube.Transform.Basis.Rotated(Vector3.UnitX, -e.motion.yrel * 0.005f);
						break;

					case SDL_EventType.SDL_MOUSEWHEEL:
						distance -= e.wheel.y * 64;
						break;

					case SDL_EventType.SDL_MOUSEBUTTONDOWN:
						if (e.button.button == SDL_BUTTON_LEFT) mouseCaptured = true;
						break;

					case SDL_EventType.SDL_MOUSEBUTTONUP:
						if (e.button.button == SDL_BUTTON_LEFT) mouseCaptured = false;
						break;

					case SDL_EventType.SDL_KEYDOWN:
						switch (e.key.keysym.scancode)
						{
							case SDL_Scancode.SDL_SCANCODE_SPACE:
								double total = 0;
								foreach (double frametime in frametimeQueue)
								{
									total += frametime;
								}

								double avgFrametime = total/256;
								Console.WriteLine($"{avgFrametime * 1000}ms ({1/avgFrametime}FPS)");
								break;
						}

						break;
				}
			}

			myCanvas.Clear();

			cube.Transform.Translation = Vector3.UnitZ * distance;
			cube.RenderCascading(myRasterizer, myCanvas, new Transform3(Basis3.Identity, Vector3.Zero));
			
			myCanvas.UploadToSDLTexture(SDLTexture);

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