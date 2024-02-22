using KDR;
using System.Numerics;
using static SDL2.SDL;

class RootProcess : Processor
{
    public readonly List<SDL_Scancode> KeysHeld = new();
    bool MouseCaptured = false;

    Vector3 CameraPosition = new(0, 0, -500);
    float Yaw = 0;
    float Pitch = 0;

    Texture Marble, SpriteFont;

    public override void OnTreeEnter()
    {
        Marble = new Texture("assets/images/marble.jpg");
        SpriteFont = new Texture("assets/images/test spritefont.png");

        // Cube
        TextureMap cubeShader = new(Marble);
        Material<TextureMap> cubeMaterial = new(cubeShader);

        Entity cube = new();
        cube
            .SetComponent<Spatial>(new Model(
                Program.OpaqueGeometryBuffer,
                MeshBuilder.BuildFromFile("assets/geometry/cube.mesh"),
                cubeMaterial
            ));

        cube.GetComponent<Spatial>().Transform.Translation = 200 * Vector3.UnitZ;

        ComposingEntity.AddChild(cube);

        // Rectangle 1
        Material<TileMap> rectMaterial = new(new TileMap(SpriteFont, 7, 5));
        rectMaterial.Shader.Index = 0;

        Entity rect = new();
        rect
            .SetComponent<Spatial>(new Model(
                Program.OpaqueGeometryBuffer,
                MeshBuilder.CreateRectangleMesh(7 * 20, 9 * 20),
                rectMaterial
            ));

        ComposingEntity.AddChild(rect);
    }

    public override void OnTreeExit()
    {
        Marble.Dispose();
        SpriteFont.Dispose();
    }

    public override void Process(float delta)
    {
        ProcessInput();

        Vector3 velocity = Vector3.Zero;

        if(KeysHeld.Contains(SDL_Scancode.SDL_SCANCODE_W)) velocity.Z += 1;
        if(KeysHeld.Contains(SDL_Scancode.SDL_SCANCODE_S)) velocity.Z -= 1;
        if(KeysHeld.Contains(SDL_Scancode.SDL_SCANCODE_D)) velocity.X += 1;
        if(KeysHeld.Contains(SDL_Scancode.SDL_SCANCODE_A)) velocity.X -= 1;

        if(KeysHeld.Contains(SDL_Scancode.SDL_SCANCODE_SPACE)) velocity.Y += 1;
        if(KeysHeld.Contains(SDL_Scancode.SDL_SCANCODE_LCTRL)) velocity.Y -= 1;

        if(velocity != Vector3.Zero) velocity = Vector3.Normalize(velocity.Rotated(-Vector3.UnitY, Yaw)) * 600 * delta;
        CameraPosition += velocity;

        Basis3 worldToView = Basis3.Identity.Rotated(Vector3.UnitY, Yaw).Rotated(Vector3.UnitX, Pitch);
        
        ComposingEntity.GetComponent<Spatial>().Transform = new(
            worldToView,
            -(worldToView * CameraPosition)
        );
    }

    void ProcessInput()
    {
        while (SDL_PollEvent(out SDL_Event e) != 0)
        {
            switch (e.type)
            {
                case SDL_EventType.SDL_QUIT:
                    Program.Quit();
                    break;

                case SDL_EventType.SDL_MOUSEMOTION:
                    if (!MouseCaptured) break;
                    Yaw -= e.motion.xrel * 0.002f;
                    Pitch -= e.motion.yrel * 0.002f;

                    break;

                case SDL_EventType.SDL_KEYDOWN:
                    if(!KeysHeld.Contains(e.key.keysym.scancode)) KeysHeld.Add(e.key.keysym.scancode);

                    switch (e.key.keysym.scancode)
                    {
                        case SDL_Scancode.SDL_SCANCODE_ESCAPE:
                            MouseCaptured = !MouseCaptured;
                            SDL_SetRelativeMouseMode((SDL_bool)Convert.ToInt32(MouseCaptured));
                            break;
                    }

                    break;
                
                case SDL_EventType.SDL_KEYUP:
                    if(KeysHeld.Contains(e.key.keysym.scancode)) KeysHeld.Remove(e.key.keysym.scancode);
                    break;
            }
        }
    }
}
