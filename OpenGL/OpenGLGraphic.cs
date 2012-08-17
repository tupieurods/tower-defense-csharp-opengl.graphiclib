using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using GraphicLib.Interfaces;
using GraphicLib.OpenGL;
using OpenTK;
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

    private readonly ShaderProgram _textureShader;

    private const float ColorFloat = 1.0f / 255.0f;

    /// <summary>
    /// Memory leak fix
    /// </summary>
    private readonly float[] _positions = new float[2 * 8];

    public OpenGLGraphic(Size windowSize)
    {
      _windowSize = windowSize;
      _simpleShader = new ShaderProgram(Properties.Resources.SimpleVertexShader, Properties.Resources.SimpleFragmentShader, new VAO());
      _textureShader = new ShaderProgram(Properties.Resources.SimpleTextureVertex, Properties.Resources.SimpleTextureFragment, new VAO());
      float[] projectionMatrix = new float[16];
      Matrix.Matrix4Ortho(projectionMatrix, 0, _windowSize.Width - 1, _windowSize.Height - 1, 0, -1, 1);
      _simpleShader.UniformMatrix4("projectionMatrix", projectionMatrix, true);
      _textureShader.UniformMatrix4("projectionMatrix", projectionMatrix, true);

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
      float[] projectionMatrix = new float[16];
      Matrix.Matrix4Ortho(projectionMatrix, 0, _windowSize.Width - 1, _windowSize.Height - 1, 0, -1, 1);
      _simpleShader.UniformMatrix4("projectionMatrix", projectionMatrix, true);
      _textureShader.UniformMatrix4("projectionMatrix", projectionMatrix, true);
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
      FillRectangle(brush, (float)x, y, width, height);
    }

    /// <summary>
    /// Fills the rectangle.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void FillRectangle(SolidBrush brush, float x, float y, float width, float height)
    {
      //It's not looks grate, but it fix memory leak
      _positions[0] = x; _positions[1] = y;
      _positions[2] = x + width; _positions[3] = y;
      _positions[4] = x + width; _positions[5] = y + height;
      _positions[6] = x; _positions[7] = y + height;
      _simpleShader.Uniform3("fragmentColor", new Vector3(brush.Color.R * ColorFloat, brush.Color.G * ColorFloat, brush.Color.B * ColorFloat));
      _simpleShader.ChangeData(VBOdata.Positions, _positions);
      _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _simpleShader.DrawArrays(BeginMode.Polygon, 0, 4);
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
      //It's not looks grate, but it fix memory leak
      List<float> pos = new List<float>();
      float xc = x + width / 2;
      float yc = y + height / 2;
      float A = width / 2.0f;
      float B = height / 2.0f;
      for (int i = 0; i <= 360; i++)
      {
        pos.Add(xc + (float)Math.Cos(i * Math.PI / 180) * A);
        pos.Add(yc + (float)Math.Sin(i * Math.PI / 180) * B);
      }
      _simpleShader.Uniform3("fragmentColor", new Vector3(brush.Color.R * ColorFloat, brush.Color.G * ColorFloat, brush.Color.B * ColorFloat));
      _simpleShader.ChangeData(VBOdata.Positions, pos.ToArray());
      _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _simpleShader.DrawArrays(BeginMode.Polygon, 0, pos.Count / 2);
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
      GL.Enable(EnableCap.Texture2D);
      GL.Enable(EnableCap.Blend);
      GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

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
      _positions[0] = x; _positions[1] = y;
      _positions[2] = x + width; _positions[3] = y;
      _positions[4] = x + width; _positions[5] = y + height;
      _positions[6] = x; _positions[7] = y + height;
      _textureShader.ChangeData(VBOdata.Positions, _positions);
      _textureShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _positions[0] = 0.0f; _positions[1] = 0.0f;
      _positions[2] = 1.0f; _positions[3] = 0.0f;
      _positions[4] = 1.0f; _positions[5] = 1.0f;
      _positions[6] = 0.0f; _positions[7] = 1.0f;
      _textureShader.ChangeData(VBOdata.TextureCoord, _positions);
      _textureShader.ChangeAttribute(VBOdata.TextureCoord, "texcoord", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      tmp.Bind();
      _textureShader.Uniform1("Texture", tmp.GlHandle);
      _textureShader.DrawArrays(BeginMode.Polygon, 0, 4);
      GL.Disable(EnableCap.Blend);
      GL.Disable(EnableCap.Texture2D);
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
      DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
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
      //It's not looks grate, but it fix memory leak
      _positions[0] = x1; _positions[1] = y1;
      _positions[2] = x2; _positions[3] = y2;
      _simpleShader.Uniform3("fragmentColor", new Vector3(pen.Color.R * ColorFloat, pen.Color.G * ColorFloat, pen.Color.B * ColorFloat));
      _simpleShader.ChangeData(VBOdata.Positions, _positions);
      _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      GL.LineWidth(pen.Width);
      _simpleShader.DrawArrays(BeginMode.Lines, 0, 2);
    }

    /// <summary>
    /// Draws the rectangle.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="rect">The rect.</param>
    public void DrawRectangle(Pen pen, Rectangle rect)
    {
      DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
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
      var brush = new SolidBrush(pen.Color);
      FillRectangle(brush, x - pen.Width / 2, y - pen.Width / 2, width + pen.Width, pen.Width);
      FillRectangle(brush, x + width - pen.Width / 2, y - pen.Width / 2, pen.Width, height + pen.Width);
      FillRectangle(brush, x - pen.Width / 2, y + height - pen.Width / 2, width + pen.Width, pen.Width);
      FillRectangle(brush, x - pen.Width / 2, y - pen.Width / 2, pen.Width, height + pen.Width);
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
      //It's not looks grate, but it fix memory leak
      List<float> pos = new List<float>();

      /*float Amax = 4 / (width * width);
      float Amin = 4 / ((width - pen.Width - 1) * (width - pen.Width - 1));
      float Bmax = 4 / (height * height);
      float Bmin = 4 / ((height - pen.Width - 1) * (height - pen.Width - 1));*/
      float xc = x + width / 2;
      float yc = y + height / 2;
      width /= 2;
      height /= 2;
      for (int i = 0; i <= 360; i++)
      {
        pos.Add(xc + (float)Math.Cos(i * Math.PI / 180) * width);
        pos.Add(yc + (float)Math.Sin(i * Math.PI / 180) * height);
        pos.Add(xc + (float)Math.Cos(i * Math.PI / 180) * (width - pen.Width));
        pos.Add(yc + (float)Math.Sin(i * Math.PI / 180) * (height - pen.Width));
      }
      _simpleShader.Uniform3("fragmentColor", new Vector3(pen.Color.R * ColorFloat, pen.Color.G * ColorFloat, pen.Color.B * ColorFloat));
      _simpleShader.ChangeData(VBOdata.Positions, pos.ToArray());
      _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _simpleShader.DrawArrays(BeginMode.TriangleStrip, 0, pos.Count / 2);
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


  //TODO remove this class to another file
  internal sealed class Texture : IDisposable
  {
    internal int GlHandle { get; private set; }
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

      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
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
