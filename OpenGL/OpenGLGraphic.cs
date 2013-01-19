using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using GraphicLib.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace GraphicLib.OpenGL
{
  /// <summary>
  /// Implements IGraphic for VideoCard with OpenGL
  /// </summary>
  public class OpenGLGraphic : IGraphic
  {

    /// <summary>
    /// Texture cache
    /// </summary>
    private static readonly Dictionary<int, Texture> Cache = new Dictionary<int, Texture>();

    /// <summary>
    /// Size of rendering window
    /// </summary>
    private Size _windowSize;

    /// <summary>
    /// Simple shader, without effects
    /// </summary>
    private readonly ShaderProgram _simpleShader;

    private readonly ShaderProgram _ellipseShader;

    /// <summary>
    /// Texture shader, without effects
    /// </summary>
    private readonly ShaderProgram _textureShader;

    /// <summary>
    /// constant for color transfering
    /// </summary>
    private const float ColorFloat = 1.0f / 255.0f;

    #region Order data
    /// <summary>
    /// Rendering order action
    /// </summary>
    private readonly List<DrawActions> _actions = new List<DrawActions>();

    /// <summary>
    /// Vertex positions for simple shader
    /// </summary>
    private readonly List<float> _vertexCoords = new List<float>();

    /// <summary>
    /// Color for fill rectangle method
    /// </summary>
    private readonly List<Vector4> _fillRectangleColors = new List<Vector4>();

    /// <summary>
    /// Texture positions
    /// </summary>
    private readonly List<float> _drawImageCoords = new List<float>();
    /// <summary>
    /// InTexture positions
    /// </summary>
    private readonly List<float> _drawImageTextureCoords = new List<float>();
    /// <summary>
    /// Textures for draw image method
    /// </summary>
    private readonly List<Texture> _drawImageTextures = new List<Texture>();

    /// <summary>
    /// Pens for draw line method
    /// </summary>
    private readonly List<Pen> _drawLinesPens = new List<Pen>();

    /// <summary>
    /// Data for FillEllipseReal method
    /// </summary>
    private readonly List<float> _ellipseCoords = new List<float>();

    /// <summary>
    /// Data for DrawEllipseReal method
    /// </summary>
    private readonly List<EllipseData> _ellipseData = new List<EllipseData>();

    /// <summary>
    /// Clipping area rectangles
    /// </summary>
    private readonly List<Rectangle> _clipAreaRects = new List<Rectangle>();

    //To prevent array recreating
    private readonly float[] _ellipseConfiguration = new float[9];
    #endregion

    public OpenGLGraphic(Size windowSize)
    {
      _simpleShader = new ShaderProgram(Properties.Resources.SimpleVertexShader, Properties.Resources.SimpleFragmentShader, new VAO());
      _textureShader = new ShaderProgram(Properties.Resources.SimpleTextureVertex, Properties.Resources.SimpleTextureFragment, new VAO());
      _ellipseShader = new ShaderProgram(Properties.Resources.SimpleVertexShader, Properties.Resources.EllipseFragment, new VAO());
      Resize(windowSize.Width, windowSize.Height, 1.0f);
    }

    #region private section

    /// <summary>
    /// Draws Texture(add draw image action to order)
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    private void DrawTexture(Texture image, int x, int y, int width, int height)
    {
      _drawImageCoords.Add(x); _drawImageCoords.Add(y);
      _drawImageCoords.Add(x + width); _drawImageCoords.Add(y);
      _drawImageCoords.Add(x + width); _drawImageCoords.Add(y + height);
      _drawImageCoords.Add(x); _drawImageCoords.Add(y + height);

      _drawImageTextureCoords.Add(0.0f); _drawImageTextureCoords.Add(0.0f);
      _drawImageTextureCoords.Add(1.0f); _drawImageTextureCoords.Add(0.0f);
      _drawImageTextureCoords.Add(1.0f); _drawImageTextureCoords.Add(1.0f);
      _drawImageTextureCoords.Add(0.0f); _drawImageTextureCoords.Add(1.0f);

      _drawImageTextures.Add(image);

      _actions.Add(DrawActions.DrawImage);
    }

    /// <summary>
    /// Draw line in order
    /// </summary>
    /// <param name="index">Line index</param>
    /// <param name="offset">Coords VBO offset</param>
    private void DrawLineReal(int index, int offset)
    {
      _simpleShader.Uniform4("fragmentColor",
        _drawLinesPens[index].Color.R * ColorFloat,
        _drawLinesPens[index].Color.G * ColorFloat,
        _drawLinesPens[index].Color.B * ColorFloat,
        _drawLinesPens[index].Color.A * ColorFloat);
      GL.LineWidth(_drawLinesPens[index].Width);
      _simpleShader.DrawArrays(BeginMode.Lines, offset, 2);
    }

    /// <summary>
    /// Fills the rectangle in order
    /// </summary>
    /// <param name="index">Filled rectangle index</param>
    /// <param name="offset">Coords VBO offset</param>
    private void FillRectangleReal(int index, int offset)
    {
      _simpleShader.Uniform4("fragmentColor", _fillRectangleColors[index]);
      _simpleShader.DrawArrays(BeginMode.Polygon, offset, 4);
    }

    /// <summary>
    /// Draws the image in order
    /// </summary>
    /// <param name="index">Image data index</param>
    /// <param name="offset">Coords VBO offset</param>
    private void DrawImageReal(int index, int offset)
    {
      _drawImageTextures[index].Bind();
      _textureShader.Uniform1("Texture", _drawImageTextures[index].GlHandle);
      _textureShader.DrawArrays(BeginMode.Polygon, offset, 4);
      if (_drawImageTextures[index].DisposeAfterFirstUse)
        _drawImageTextures[index].Dispose();
    }

    private void AddEllipseDrawingTask(float x, float y, float width, float height, float border, Color color)
    {
      float widthMax = width + border + 5.0f;//+1 for AA support
      float heightMax = height + border + 5.0f;
      float xc = x + width / 2;
      float yc = y + height / 2;
      _ellipseCoords.Add(xc - widthMax / 2); _ellipseCoords.Add(yc - heightMax / 2);
      _ellipseCoords.Add(xc + widthMax / 2); _ellipseCoords.Add(yc - heightMax / 2);
      _ellipseCoords.Add(xc + widthMax / 2); _ellipseCoords.Add(yc + heightMax / 2);
      _ellipseCoords.Add(xc - widthMax / 2); _ellipseCoords.Add(yc + heightMax / 2);
      _actions.Add(DrawActions.DrawEllipse);
      _ellipseData.Add(new EllipseData { color = color, Xc = xc, Yc = yc, Xr = width / 2, Yr = height / 2, border = border });
    }

    /// <summary>
    /// Draws the ellipse in order
    /// </summary>
    /// <param name="index">ellipse data index</param>
    /// <param name="offset">Coords VBO offset</param>
    private void DrawEllipseReal(int index, int offset)
    {
      _ellipseConfiguration[0] = _ellipseData[index].color.R * ColorFloat;
      _ellipseConfiguration[1] = _ellipseData[index].color.G * ColorFloat;
      _ellipseConfiguration[2] = _ellipseData[index].color.B * ColorFloat;
      _ellipseConfiguration[3] = _ellipseData[index].Xc;
      _ellipseConfiguration[4] = _windowSize.Height - _ellipseData[index].Yc;
      _ellipseConfiguration[5] = 0;
      _ellipseConfiguration[6] = _ellipseData[index].Xr;
      _ellipseConfiguration[7] = _ellipseData[index].Yr;
      _ellipseConfiguration[8] = _ellipseData[index].border;
      _ellipseShader.Uniform3("conf", 3, _ellipseConfiguration);
      _ellipseShader.DrawArrays(BeginMode.Polygon, offset, 4);
    }
    #endregion

    #region Implementation of IGraphic

    /// <summary>
    /// Resizes drawing area
    /// Don't Forget to change Clip area after resize
    /// </summary>
    /// <param name="width">The base width.</param>
    /// <param name="height">The base height.</param>
    /// <param name="scaling">The scaling.</param>
    /// <param name="drawingContainer">The drawing container(using only for Windows forms).</param>
    /// <returns>Returns true if succefull resized</returns>
    public bool Resize(int width, int height, float scaling, object drawingContainer = null)
    {
      _windowSize = new Size(Convert.ToInt32(width * scaling), Convert.ToInt32(height * scaling));
      Matrix4 projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, _windowSize.Width - 1, _windowSize.Height - 1, 0, -1, 1);
      _simpleShader.UniformMatrix4("projectionMatrix", projectionMatrix, false);
      _ellipseShader.UniformMatrix4("projectionMatrix", projectionMatrix, false);
      _textureShader.UniformMatrix4("projectionMatrix", projectionMatrix, false);
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
        _clipAreaRects.Add(value);
        _actions.Add(DrawActions.ClipArea);
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
      _vertexCoords.Add(x); _vertexCoords.Add(y);
      _vertexCoords.Add(x + width); _vertexCoords.Add(y);
      _vertexCoords.Add(x + width); _vertexCoords.Add(y + height);
      _vertexCoords.Add(x); _vertexCoords.Add(y + height);
      _fillRectangleColors.Add(new Vector4(brush.Color.R * ColorFloat, brush.Color.G * ColorFloat, brush.Color.B * ColorFloat, brush.Color.A * ColorFloat));
      _actions.Add(DrawActions.FillRectangle);
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
      AddEllipseDrawingTask(x, y, width, height, 0.0f, brush.Color);
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
      Size textSize = TextRenderer.MeasureText(s, font);
      Bitmap tmp = new Bitmap(textSize.Width, textSize.Height);
      var canva = Graphics.FromImage(tmp);
      canva.Clear(Color.Transparent);
      canva.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
      canva.DrawString(s, font, brush, 0, 0);
      DrawTexture(new Texture(tmp, true), (int)point.X, (int)point.Y, tmp.Width, tmp.Height);
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
      DrawTexture(TextureCache(image), x, y, width, height);
    }

    private static Texture TextureCache(Image image)
    {
      var img = image as Bitmap;
      if (img == null)
        throw new ArgumentNullException("image");
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
      return tmp;
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
    /// Draws the part of image
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="drawingRect">The drawing rectangle.</param>
    /// <param name="imageRect">Drawing image part rectangle</param>
    public void DrawImagePart(Image image, Rectangle drawingRect, Rectangle imageRect)
    {
      Texture tex = TextureCache(image);
      _drawImageCoords.Add(drawingRect.X); _drawImageCoords.Add(drawingRect.Y);
      _drawImageCoords.Add(drawingRect.X + drawingRect.Width); _drawImageCoords.Add(drawingRect.Y);
      _drawImageCoords.Add(drawingRect.X + drawingRect.Width); _drawImageCoords.Add(drawingRect.Y + drawingRect.Height);
      _drawImageCoords.Add(drawingRect.X); _drawImageCoords.Add(drawingRect.Y + drawingRect.Height);

      _drawImageTextureCoords.Add((float)imageRect.X / image.Width); _drawImageTextureCoords.Add((float)imageRect.Y / image.Height);
      _drawImageTextureCoords.Add((float)imageRect.Width / image.Width); _drawImageTextureCoords.Add((float)imageRect.Y / image.Height);
      _drawImageTextureCoords.Add((float)imageRect.Width / image.Width); _drawImageTextureCoords.Add((float)imageRect.Height / image.Height);
      _drawImageTextureCoords.Add((float)imageRect.X / image.Width); _drawImageTextureCoords.Add((float)imageRect.Height / image.Height);

      _drawImageTextures.Add(tex);

      _actions.Add(DrawActions.DrawImage);
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
      _vertexCoords.Add(x1); _vertexCoords.Add(y1);
      _vertexCoords.Add(x2); _vertexCoords.Add(y2);
      _drawLinesPens.Add(pen);
      _actions.Add(DrawActions.DrawLine);
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
      if (Math.Abs(pen.Width - Math.Min(width, height)) < 0.1)
      {
        //Filling ellipse
        AddEllipseDrawingTask(x, y, width, height, 0, pen.Color);
        return;
      }
      AddEllipseDrawingTask(x, y, width, height, pen.Width > 1.0f ? pen.Width : 1.0f, pen.Color);
    }

    /// <summary>
    /// Renders this instance.
    /// </summary>
    public void Render()
    {
      GL.Enable(EnableCap.ScissorTest);
      GL.Enable(EnableCap.Texture2D);
      GL.Enable(EnableCap.Blend);
      GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

      #region Uploading data to VBO

      #region Simple shader
      _simpleShader.ChangeData(VBOdata.Positions, _vertexCoords.ToArray());
      _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      #endregion

      #region Texture shader
      _textureShader.ChangeData(VBOdata.Positions, _drawImageCoords.ToArray());
      _textureShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _textureShader.ChangeData(VBOdata.TextureCoord, _drawImageTextureCoords.ToArray());
      _textureShader.ChangeAttribute(VBOdata.TextureCoord, "texcoord", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      #endregion

      #region Ellipse shader
      _ellipseShader.ChangeData(VBOdata.Positions, _ellipseCoords.ToArray());
      _ellipseShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      #endregion

      #endregion

      #region Offsets and drawing methods indexes initializers
      int vboOffset = 0;
      int textureVBOoffset = 0;
      int ellipseVBOoffset = 0;
      int clipAreaNumber = 0;
      int fillRectangleNumber = 0;
      int drawLineNumber = 0;
      int drawImageNumber = 0;
      int drawEllipseNumber = 0;
      #endregion

      foreach (var drawAction in _actions)
      {
        switch (drawAction)
        {
          case DrawActions.DrawImage:
            DrawImageReal(drawImageNumber, textureVBOoffset);
            textureVBOoffset += 4;
            drawImageNumber++;
            break;
          case DrawActions.DrawLine:
            DrawLineReal(drawLineNumber, vboOffset);
            vboOffset += 2;
            drawLineNumber++;
            break;
          case DrawActions.DrawEllipse:
            DrawEllipseReal(drawEllipseNumber, ellipseVBOoffset);
            ellipseVBOoffset += 4;
            drawEllipseNumber++;
            break;
          case DrawActions.FillRectangle:
            FillRectangleReal(fillRectangleNumber, vboOffset);
            vboOffset += 4;
            fillRectangleNumber++;
            break;
          case DrawActions.FillEllipse:
            DrawEllipseReal(drawEllipseNumber, ellipseVBOoffset);
            ellipseVBOoffset += 4;
            drawEllipseNumber++;
            break;
          case DrawActions.ClipArea:
            GL.Scissor(_clipAreaRects[clipAreaNumber].X,
              _windowSize.Height - _clipAreaRects[clipAreaNumber].Height - _clipAreaRects[clipAreaNumber].Y,
              _clipAreaRects[clipAreaNumber].Width,
              _clipAreaRects[clipAreaNumber].Height);
            clipAreaNumber++;
            break;
          case DrawActions.DrawString:
            //Currently not used
            break;
          case DrawActions.DrawRectangle:
            //Currently not used
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
      #region data clearing
      _fillRectangleColors.Clear();
      _vertexCoords.Clear();
      _drawImageTextures.Clear();
      _drawImageCoords.Clear();
      _drawImageTextureCoords.Clear();
      _ellipseData.Clear();
      _ellipseCoords.Clear();
      _actions.Clear();
      _drawLinesPens.Clear();
      _clipAreaRects.Clear();
      #endregion

      GL.Disable(EnableCap.Blend);
      GL.Disable(EnableCap.Texture2D);
      GL.Disable(EnableCap.ScissorTest);
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
      FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), x, y, width, height);
    }

    #endregion
  }
}
