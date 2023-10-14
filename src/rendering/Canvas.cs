using static SDL2.SDL;
using System.Runtime.InteropServices;

public class Canvas
{
    public int[] FrameBuffer;
    public float[] DepthBuffer;

    public readonly int Width, Height, Length, Pitch;

    public Canvas(int Width, int Height)
    {
        this.Width = Width;
        this.Height = Height;
        this.Length = Width * Height;
        this.Pitch = Width * sizeof(int);
        
        this.FrameBuffer = new int[Width * Height];
        this.DepthBuffer = new float[Width * Height];
    }

    public void Clear()
    {
        Array.Clear(FrameBuffer);
        Array.Fill(DepthBuffer, float.MaxValue);
    }

    public unsafe void PushToSurface(IntPtr Surface) => Marshal.Copy(FrameBuffer, 0, ((SDL_Surface*)Surface)->pixels, Length);

    public unsafe void UploadToSDLTexture(IntPtr Texture)
    {
        fixed(void* BufferPtr = &FrameBuffer[0])
        {
            SDL_UpdateTexture(Texture, 0, (IntPtr)BufferPtr, Pitch);
        }
    }
}