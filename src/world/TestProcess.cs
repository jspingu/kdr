using System.Numerics;

class TestProcess : Processor
{
    RootProcess RootProc;
    Spatial ThisSpatial;

    public override void OnTreeEnter()
    {
        RootProc = (RootProcess)ThisEntity.Root.GetComponent<Processor>();
        ThisSpatial = ThisEntity.GetComponent<Spatial>();
    }

    public override void Process(float delta)
    {
        if(RootProc.KeysHeld.Contains(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
        {
            ThisSpatial.Transform.Basis.Rotate(Vector3.Normalize(Vector3.One), delta);
        }
    }
}