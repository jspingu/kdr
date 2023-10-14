using static SDL2.SDL;
using System.Runtime.InteropServices;

public class Canvas
{
    public int[] FrameBuffer;
    public float[] DepthBuffer;

    public readonly int Width, Height, Length, Pitch;

    public Canvas(int width, int height)
    {
        Width = width;
        Height = height;
        
        Length = width * height;
        Pitch = width * sizeof(int);
        
        FrameBuffer = new int[width * height];
        DepthBuffer = new float[width * height];
    }

    public void Clear()
    {
        Array.Clear(FrameBuffer);
        Array.Fill(DepthBuffer, float.MaxValue);
    }

    public unsafe void PushToSurface(IntPtr surface) => Marshal.Copy(FrameBuffer, 0, ((SDL_Surface*)surface)->pixels, Length);

    public unsafe void UploadToSDLTexture(IntPtr texture)
    {
        fixed(void* BufferPtr = &FrameBuffer[0])
        {
            SDL_UpdateTexture(texture, 0, (IntPtr)BufferPtr, Pitch);
        }
    }
}