using KDR;
using static SDL2.SDL;

class TestProcess : Processor
{
    RootProcess RootProc;
    Model ThisModel;

    int a = 0;

    public override void OnTreeEnter()
    {
        RootProc = (RootProcess)ComposingEntity.Root.GetComponent<Processor>();
        ThisModel = (Model)ComposingEntity.GetComponent<Spatial>();

        //Console.WriteLine(SDL_GetScancodeName(SDL_Scancode.SDL_SCANCODE_0));
        Console.WriteLine(++a);
    }

    public override void Process(float delta)
    {
        Material<TileMap> modelMaterial = (Material<TileMap>)ThisModel.Material;
        if (RootProc.KeysHeld.Contains(SDL_Scancode.SDL_SCANCODE_LEFT)) modelMaterial.Shader.Index--;
        else if (RootProc.KeysHeld.Contains(SDL_Scancode.SDL_SCANCODE_RIGHT)) modelMaterial.Shader.Index++;
    }
}
