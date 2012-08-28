using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGl
{
  internal sealed class Texture : IDisposable
  {
    internal int GlHandle { get; private set; }
    private int Width { get; set; }
    private int Height { get; set; }
    internal bool DisposeAfterFirstUse { get; private set; }

    public Texture(Bitmap bitmap, bool disposeAfterFirstUse = false)
    {
      GlHandle = GL.GenTexture();
      Bind();
      DisposeAfterFirstUse = disposeAfterFirstUse;

      Width = bitmap.Width;
      Height = bitmap.Height;

      var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
      bitmap.UnlockBits(bitmapData);

      if (!DisposeAfterFirstUse)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);

      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    public void Bind()
    {
      GL.BindTexture(TextureTarget.Texture2D, GlHandle);
    }

    #region Disposable

    private bool _disposed;

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      if (_disposed)
        return;
      if (disposing)
      {
        GL.DeleteTexture(GlHandle);
      }
      _disposed = true;
    }

    ~Texture()
    {
      Dispose(false);
    }

    #endregion
  }
}