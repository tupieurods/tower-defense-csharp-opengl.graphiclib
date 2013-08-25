using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL
{
  public sealed class Texture: IDisposable
  {
    public int GlHandle { get; private set; }
    public int Width { get; set; }
    public int Height { get; set; }
    internal bool DisposeAfterFirstUse { get; private set; }

    public Texture(Bitmap bitmap, bool disposeAfterFirstUse = false,
        bool setLinearFilter = false, bool setNearestFilter = true)
    {
      GlHandle = GL.GenTexture();
      //Bind();
      GL.BindTexture(TextureTarget.Texture2D, GlHandle);
      DisposeAfterFirstUse = disposeAfterFirstUse;

      Width = bitmap.Width;
      Height = bitmap.Height;

      var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                                       System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
      bitmap.UnlockBits(bitmapData);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 3);
      if(setLinearFilter)
      {
        SetLinearFilter();
      }
      if(setNearestFilter)
      {
        this.setNearestFilter();
      }
    }

    public void SetLinearFilter()
    {
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }

    public void setNearestFilter()
    {
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    public void Bind(TextureUnit textureUnit)
    {
      GL.ActiveTexture(textureUnit);
      GL.BindTexture(TextureTarget.Texture2D, GlHandle);
    }

    public override int GetHashCode()
    {
      return GlHandle;
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
      if(_disposed)
      {
        return;
      }
      if(disposing)
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