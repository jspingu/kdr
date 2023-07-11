using static SDL2.SDL;
public class Program
{
	public static void Main()
	{
		SDL_Init(SDL_INIT_VIDEO);

		var window = SDL_CreateWindow(
			"Title",
			SDL_WINDOWPOS_CENTERED,
			SDL_WINDOWPOS_CENTERED,
			1280,
			720,
			SDL_WindowFlags.SDL_WINDOW_RESIZABLE
		);

		IntPtr ScreenSurface = SDL_GetWindowSurface(window);
		Canvas MyCanvas = new Canvas(1280, 720);
		
		Primitive CatPrim = new Primitive(new Vector2(500, 300), new Vector2(200, 0), new Vector2(100, 200));
		
		IntPtr Cat = SDL_LoadBMP("images/cat.bmp");
		var CatShader = new AffineTextureMap(Cat, CatPrim);

		SDL_Event e;
		bool quit = false;

		ulong CountOld = SDL_GetPerformanceCounter();

		while (!quit)
		{
			double TimeDelta = (double) (SDL_GetPerformanceCounter() - CountOld) / SDL_GetPerformanceFrequency();
			CountOld = SDL_GetPerformanceCounter();

			while (SDL_PollEvent(out e) != 0)
			{
				switch (e.type)
				{
					case SDL_EventType.SDL_QUIT:
						quit = true;
						break;

					case SDL_EventType.SDL_KEYDOWN:
						if (e.key.keysym.scancode == SDL_Scancode.SDL_SCANCODE_SPACE)
						{
							Console.WriteLine(1/TimeDelta + " fps");
						}
						if (e.key.keysym.scancode == SDL_Scancode.SDL_SCANCODE_A)
						{
							CatPrim.Origin.x -= 5;
						}
						break;
				}
			}

			MyCanvas.Clear();
			CatShader.Prim = CatPrim;
			MyCanvas.DrawPrimitive(CatPrim, CatShader);
			MyCanvas.PushToSurface(ScreenSurface);
			SDL_UpdateWindowSurface(window);

			// SDL_Delay(10);
		}

		SDL_DestroyWindow(window);
		SDL_Quit();
	}
}