using static SDL2.SDL;
using System.Numerics;
using SDL2;

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

		Rasterizer myRasterizer = new PerspectiveRasterizer(RenderWidth, RenderHeight, 5f, 1000f, MathF.PI / 2f);
		Canvas myCanvas = new(RenderWidth, RenderHeight);
		GeometryBuffer OpaqueGeometryBuffer = new();

		Entity world = new();
		Entity myCube = new();

		IntPtr texture = SDL_LoadBMP("images/wood.bmp");
		TextureMap cubeShader = new(texture);
		Material<TextureMap> cubeMaterial = new(cubeShader);

		world
			.SetComponent<Processor>(new WorldProcess())
			.SetComponent<Spatial>(new())
			.OnTreeEnter(world);

		myCube
			.SetComponent<Spatial>(new Model(
				OpaqueGeometryBuffer,
				MeshBuilder.BuildFromFile("assets/cube.mesh"),
				cubeMaterial
			));

		world.AddChild(myCube);

		float yaw = 0;
		float pitch = 0;

		Vector3 cameraPos = new(0, 0, -500);

		List<SDL_Scancode> keysHeld = new();

        while (!quit)
		{
			double delta = (double) (SDL_GetPerformanceCounter() - countOld) / SDL_GetPerformanceFrequency();
			countOld = SDL_GetPerformanceCounter();

			frametimeQueue.Enqueue(delta);
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

						yaw -= e.motion.xrel * 0.002f;
						pitch -= e.motion.yrel * 0.002f;

						break;

					// case SDL_EventType.SDL_MOUSEBUTTONDOWN:
					// 	if (e.button.button == SDL_BUTTON_LEFT) mouseCaptured = true;
					// 	break;

					// case SDL_EventType.SDL_MOUSEBUTTONUP:
					// 	if (e.button.button == SDL_BUTTON_LEFT) mouseCaptured = false;
					// 	break;

					case SDL_EventType.SDL_KEYDOWN:
						if(!keysHeld.Contains(e.key.keysym.scancode)) keysHeld.Add(e.key.keysym.scancode);

						switch (e.key.keysym.scancode)
						{
							case SDL_Scancode.SDL_SCANCODE_ESCAPE:
								mouseCaptured = !mouseCaptured;
								SDL_SetRelativeMouseMode((SDL_bool)Convert.ToInt32(mouseCaptured));
								break;
						}

						// 	case SDL_Scancode.SDL_SCANCODE_SPACE:
						// 		double total = 0;
						// 		foreach (double frametime in frametimeQueue)
						// 		{
						// 			total += frametime;
						// 		}

						// 		double avgFrametime = total/256;
						// 		Console.WriteLine($"{avgFrametime * 1000}ms ({1/avgFrametime}FPS)");
						// 		break;
						// }

						break;
					
					case SDL_EventType.SDL_KEYUP:
						if(keysHeld.Contains(e.key.keysym.scancode)) keysHeld.Remove(e.key.keysym.scancode);
						break;
				}
			}

			myCanvas.Clear();

			Vector3 velocity = Vector3.Zero;

			if(keysHeld.Contains(SDL_Scancode.SDL_SCANCODE_W)) velocity.Z += 1;
			if(keysHeld.Contains(SDL_Scancode.SDL_SCANCODE_S)) velocity.Z -= 1;
			if(keysHeld.Contains(SDL_Scancode.SDL_SCANCODE_D)) velocity.X += 1;
			if(keysHeld.Contains(SDL_Scancode.SDL_SCANCODE_A)) velocity.X -= 1;

			if(keysHeld.Contains(SDL_Scancode.SDL_SCANCODE_SPACE)) velocity.Y += 1;
			if(keysHeld.Contains(SDL_Scancode.SDL_SCANCODE_LCTRL)) velocity.Y -= 1;

			if(velocity != Vector3.Zero) velocity = Vector3.Normalize(velocity.Rotated(-Vector3.UnitY, yaw)) * 600 * (float)delta;
			cameraPos += velocity;

			Basis3 worldToView = Basis3.Identity.Rotated(Vector3.UnitY, yaw).Rotated(Vector3.UnitX, pitch);
			world.GetComponent<Spatial>().Transform = new(
				worldToView,
				-(worldToView * cameraPos)
			);

			world.ProcessCascading((float)delta);
			world.RenderProcessCascading(Transform3.Default);

			myRasterizer.DrawScene(OpaqueGeometryBuffer, myCanvas);
			OpaqueGeometryBuffer.ResetState();
			
			myCanvas.UploadToSDLTexture(SDLTexture);

			SDL_RenderClear(SDLRenderer);
			SDL_RenderCopy(SDLRenderer, SDLTexture, 0, 0);
			SDL_RenderPresent(SDLRenderer);
		}

		SDL_FreeSurface(texture);

		SDL_DestroyWindow(SDLWindow);
		SDL_DestroyRenderer(SDLRenderer);
		SDL_DestroyTexture(SDLTexture);
		
		SDL_Quit();
	}
}