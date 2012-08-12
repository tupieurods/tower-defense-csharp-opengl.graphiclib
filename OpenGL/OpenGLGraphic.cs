using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using GraphicLib.Interfaces;
using GraphicLib.OpenGL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;
using PixelInternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;
using TextureMagFilter = OpenTK.Graphics.OpenGL.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.OpenGL.TextureMinFilter;
using TextureParameterName = OpenTK.Graphics.OpenGL.TextureParameterName;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;

namespace GraphicLib.OpenGl
{
  public class OpenGLGraphic : IGraphic
  {

    private static readonly Dictionary<int, Texture> Cache = new Dictionary<int, Texture>();

    private Size _windowSize;

    private readonly ShaderProgram _simpleShader;

    private const float ColorFloat = 1.0f / 255.0f;

    /// <summary>
    /// Memory leak fix
    /// </summary>
    private float[] _positions = new float[2 * 4];
    private float[] _colors = new float[3 * 4];

    private OpenGLGraphic()
    {
    }

    static OpenGLGraphic()
    {

    }

    public OpenGLGraphic(Size windowSize)
    {
      _windowSize = windowSize;
      _simpleShader = new ShaderProgram(Properties.Resources.SimpleVertexShader, Properties.Resources.SimpleFragmentShader, new VAO());
      float[] projectionMatrix = new float[16];
      Matrix.Matrix4Ortho(projectionMatrix, 0, _windowSize.Width, _windowSize.Height, 0, -1, 1);
      _simpleShader.UniformMatrix4("projectionMatrix", projectionMatrix, true);
    }

    #region Implementation of IGraphic

    /// <summary>
    /// Resizes drawing area
    /// </summary>
    /// <param name="width">The base width.</param>
    /// <param name="height">The base height.</param>
    /// <param name="scaling">The scaling.</param>
    /// <param name="drawingContainer">The drawing container(using only for Windows forms).</param>
    /// <returns>Returns true if succefull resized</returns>
    public bool Resize(int width, int height, float scaling, object drawingContainer = null)
    {
      _windowSize = new Size(Convert.ToInt32(width * scaling), Convert.ToInt32(height * scaling));
      return true;
    }

    /// <summary>
    /// Gets or sets the clip.
    /// </summary>
    /// <value>
    /// The clip.
    /// </value>
    public Rectangle Clip
    {
      set
      {
        GL.Enable(EnableCap.ScissorTest);
        //По идее должно быть как закоментированно, пока у OpenGL свои координаты с собственным преферансом и путанами
        //GL.Scissor(value.X, value.Y, value.Width, value.Height);
        GL.Scissor(value.X, _windowSize.Height - value.Height - value.Y, value.Width, value.Height);
      }
    }

    /// <summary>
    /// Fills the rectangle.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void FillRectangle(SolidBrush brush, int x, int y, int width, int height)
    {
      GL.Disable(EnableCap.Texture2D);
      //It's not looks grate, but it fix memory leak
      _positions[0] = x; _positions[1] = y;
      _positions[2] = x + width; _positions[3] = y;
      _positions[4] = x + width; _positions[5] = y + height;
      _positions[6] = x; _positions[7] = y + height;
      Color4 color = new Color4(brush.Color.R * ColorFloat,
                                brush.Color.G * ColorFloat,
                                brush.Color.B * ColorFloat,
                                brush.Color.A * ColorFloat);
      for (int i = 0; i < 4; i++)
      {
        _colors[i * 3] = color.R;
        _colors[i * 3 + 1] = color.G;
        _colors[i * 3 + 2] = color.B;
      }
      _simpleShader.ChangeData(VBOdata.Positions, _positions);
      _simpleShader.ChangeData(VBOdata.Color, _colors);
      _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _simpleShader.ChangeAttribute(VBOdata.Color, "color", 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      _simpleShader.DrawArrays(BeginMode.Quads, 0, 4);
      GL.Enable(EnableCap.Texture2D);
    }

    /// <summary>
    /// Fills the ellipse.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void FillEllipse(SolidBrush brush, float x, float y, float width, float height)
    {
    }

    /// <summary>
    /// Draws the string.
    /// </summary>
    /// <param name="s">The s.</param>
    /// <param name="font">The font.</param>
    /// <param name="brush">The brush.</param>
    /// <param name="point">The point.</param>
    public void DrawString(string s, Font font, Brush brush, PointF point)
    {
    }

    /// <summary>
    /// Draws the image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void DrawImage(Image image, int x, int y, int width, int height)
    {
      var img = image as Bitmap;
      if (img == null)
        return;
      int hashCode = img.GetHashCode();
      Texture tmp;
      if (!Cache.ContainsKey(hashCode))
      {
        tmp = new Texture(img);
        Cache.Add(hashCode, tmp);
      }
      else if (img.Tag != null && (int)img.Tag == 1)
      {
        Cache[hashCode].Dispose();
        tmp = new Texture(img);
        Cache[hashCode] = tmp;
      }
      else
        tmp = Cache[hashCode];
      tmp.Bind();
      GL.Color4(Color4.White);
      GL.Begin(BeginMode.Quads);

      GL.TexCoord2(0, 0);
      GL.Vertex2(x, y);

      GL.TexCoord2(1, 0);
      GL.Vertex2(x + width, y);

      GL.TexCoord2(1, 1);
      GL.Vertex2(x + width, y + height);

      GL.TexCoord2(0, 1);
      GL.Vertex2(x, y + height);

      GL.End();
    }

    /// <summary>
    /// Draws the image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="rect">The rect.</param>
    public void DrawImage(Image image, Rectangle rect)
    {
      DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    /// Draws the line.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="pt1">The PT1.</param>
    /// <param name="pt2">The PT2.</param>
    public void DrawLine(Pen pen, Point pt1, Point pt2)
    {
      GL.Disable(EnableCap.Texture2D);
      GL.Color4(pen.Color);
      GL.Begin(BeginMode.Lines);
      GL.Vertex2(pt1.X, pt1.Y);
      GL.Vertex2(pt2.X, pt2.Y);
      GL.End();
      GL.Enable(EnableCap.Texture2D);
    }

    /// <summary>
    /// Draws the line.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="x1">The x1.</param>
    /// <param name="y1">The y1.</param>
    /// <param name="x2">The x2.</param>
    /// <param name="y2">The y2.</param>
    public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
    {
      GL.Disable(EnableCap.Texture2D);
      GL.Color4(pen.Color);
      GL.Begin(BeginMode.Lines);
      GL.Vertex2(x1, y1);
      GL.Vertex2(x2, y2);
      GL.End();
      GL.Enable(EnableCap.Texture2D);
    }

    /// <summary>
    /// Draws the rectangle.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="rect">The rect.</param>
    public void DrawRectangle(Pen pen, Rectangle rect)
    {
    }

    /// <summary>
    /// Draws the rectangle.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void DrawRectangle(Pen pen, int x, int y, int width, int height)
    {
    }

    /// <summary>
    /// Draws the ellipse.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void DrawEllipse(Pen pen, float x, float y, float width, float height)
    {
    }

    /// <summary>
    /// Renders this instance.
    /// </summary>
    public void Render()
    {

    }

    /// <summary>
    /// Makes game window gray
    /// </summary>
    /// <param name="x">Gray area x start position</param>
    /// <param name="y">Gray area y start position</param>
    /// <param name="width">Gray area width</param>
    /// <param name="height">Gray area height</param>
    public void MakeGray(int x, int y, int width, int height)
    {
    }

    #endregion
  }


  internal sealed class Texture : IDisposable
  {
    private int GlHandle { get; set; }
    private int Width { get; set; }
    private int Height { get; set; }

    public Texture(Bitmap bitmap)
    {
      GlHandle = GL.GenTexture();
      Bind();

      Width = bitmap.Width;
      Height = bitmap.Height;

      var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
      bitmap.UnlockBits(bitmapData);

      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
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
