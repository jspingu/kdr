namespace KDR;

using static SDL2.SDL;
using System.Numerics;

public class Canvas
{
    public uint[] FrameBuffer;
    public float[] DepthBuffer;

    public readonly int Width, Height, Length, Pitch, Bytes;
    public readonly Vector2 Midpoint;

    public Canvas(int width, int height)
    {
        Width = width;
        Height = height;

        Midpoint = new Vector2(width, height) / 2f;
        
        Length = width * height;
        Pitch = width * sizeof(uint);
        Bytes = Length * sizeof(uint);
        
        FrameBuffer = new uint[width * height];
        DepthBuffer = new float[width * height];
    }

    public void Clear()
    {
        Array.Clear(FrameBuffer);
        Array.Fill(DepthBuffer, float.MaxValue);
    }

    public unsafe void PushToSurface(IntPtr surface) 
    {
        fixed(void* bufferPtr = &FrameBuffer[0])
        {
            Buffer.MemoryCopy(bufferPtr, (void*)((SDL_Surface*)surface)->pixels, Bytes, Bytes);
        }
    }

    public unsafe void UploadToSDLTexture(IntPtr texture)
    {
        fixed(void* bufferPtr = &FrameBuffer[0])
        {
            SDL_UpdateTexture(texture, 0, (IntPtr)bufferPtr, Pitch);
        }
    }
}
