using static SDL2.SDL;
using System.Numerics;
public class Program
{
	public static void Main()
	{
		SDL_Init(SDL_INIT_VIDEO);

		IntPtr window = SDL_CreateWindow(
			"Title",
			SDL_WINDOWPOS_CENTERED,
			SDL_WINDOWPOS_CENTERED,
			1280,
			720,
			SDL_WindowFlags.SDL_WINDOW_RESIZABLE
		);

		IntPtr ScreenSurface = SDL_GetWindowSurface(window);
		Canvas MyCanvas = new Canvas(1280, 720);
		
		Primitive MyPrim = new Primitive(new Vector2(640, 260), new Vector2(-200, 200), new Vector2(200, 200));
		RGBTriangle Rainbow = new RGBTriangle(MyPrim);

		SDL_Event e;
		bool quit = false;

		ulong CountOld = SDL_GetPerformanceCounter();

		Queue<double> FrametimeQueue = new Queue<double>();

		float Time = 0;
		float Angle = 0;

		while (!quit)
		{
			double TimeDelta = (double) (SDL_GetPerformanceCounter() - CountOld) / SDL_GetPerformanceFrequency();
			CountOld = SDL_GetPerformanceCounter();

			Time += (float) TimeDelta;

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

							case SDL_Scancode.SDL_SCANCODE_LEFT:
								Angle += 0.01f;
								break;

							case SDL_Scancode.SDL_SCANCODE_RIGHT:
								Angle -= 0.01f;
								break;

							case SDL_Scancode.SDL_SCANCODE_T:
								Console.WriteLine(MyPrim.a);
								Console.WriteLine(MyPrim.b);
								break;
						}

						break;
				}
			}

			MyPrim.a = new Vector2(-200, 200) * MathF.Cos(Angle) + new Vector2(200, 200) * MathF.Sin(Angle);

			MyCanvas.Clear();
			Rainbow.Prim = MyPrim;
			MyCanvas.DrawPrimitive(MyPrim, Rainbow);
			MyCanvas.PushToSurface(ScreenSurface);
			SDL_UpdateWindowSurface(window);
		}

		SDL_DestroyWindow(window);
		SDL_Quit();
	}
}