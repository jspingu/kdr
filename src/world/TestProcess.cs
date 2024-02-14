using KDR;
using System.Numerics;

class TestProcess : Processor
{
    RootProcess RootProc;
    Spatial ThisSpatial;

    public override void OnTreeEnter()
    {
        RootProc = (RootProcess)ComposingEntity.Root.GetComponent<Processor>();
        ThisSpatial = ComposingEntity.GetComponent<Spatial>();

        ThisSpatial.Transform.Translation = new Vector3(-300, 0, -100);
    }

    public override void Process(float delta)
    {
        if (RootProc.KeysHeld.Contains(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
        {
            ThisSpatial.Transform.Basis.Rotate(Vector3.UnitY, delta);
        }
    }
}
