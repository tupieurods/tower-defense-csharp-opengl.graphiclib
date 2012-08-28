using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using GraphicLib.Interfaces;
using GraphicLib.OpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;

namespace GraphicLib.OpenGl
{
  public class OpenGLGraphic : IGraphic
  {

    private static readonly Dictionary<int, Texture> Cache = new Dictionary<int, Texture>();

    private Size _windowSize;

    private readonly ShaderProgram _simpleShader;

    private readonly ShaderProgram _ellipseShader;

    private readonly ShaderProgram _textureShader;

    private const float ColorFloat = 1.0f / 255.0f;

    private List<DrawActions> _actions = new List<DrawActions>();
    private List<float> _vertexCoords = new List<float>();

    private List<Vector4> _fillRectangleColors = new List<Vector4>();

    private List<float> _drawImageCoords = new List<float>();
    private List<float> _drawImageTextureCoords = new List<float>();
    private List<Texture> _drawImageTextures = new List<Texture>();

    private List<Pen> _drawLinesPens = new List<Pen>();

    private List<FillEllipseData> _fillEllipseData = new List<FillEllipseData>();

    private List<DrawEllipseData> _drawEllipseData = new List<DrawEllipseData>();

    private List<Rectangle> _clipAreaRects = new List<Rectangle>();

    public OpenGLGraphic(Size windowSize)
    {
      _windowSize = windowSize;
      _simpleShader = new ShaderProgram(Properties.Resources.SimpleVertexShader, Properties.Resources.SimpleFragmentShader, new VAO());
      _textureShader = new ShaderProgram(Properties.Resources.SimpleTextureVertex, Properties.Resources.SimpleTextureFragment, new VAO());
      _ellipseShader = new ShaderProgram(Properties.Resources.SimpleVertexShader, Properties.Resources.SimpleFragmentShader, new VAO());
      float[] projectionMatrix = new float[16];
      Matrix.Matrix4Ortho(projectionMatrix, 0, _windowSize.Width - 1, _windowSize.Height - 1, 0, -1, 1);
      _simpleShader.UniformMatrix4("projectionMatrix", projectionMatrix, true);
      _ellipseShader.UniformMatrix4("projectionMatrix", projectionMatrix, true);
      _textureShader.UniformMatrix4("projectionMatrix", projectionMatrix, true);
    }

    #region private section

    /// <summary>
    /// Draws Texture
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

    private void DrawLineReal(int index, int offset)
    {
      _simpleShader.Uniform4("fragmentColor",
              new Vector4(_drawLinesPens[index].Color.R * ColorFloat,
              _drawLinesPens[index].Color.G * ColorFloat,
              _drawLinesPens[index].Color.B * ColorFloat,
              _drawLinesPens[index].Color.A * ColorFloat));
      GL.LineWidth(_drawLinesPens[index].Width);
      _simpleShader.DrawArrays(BeginMode.Lines, offset, 2);
    }

    private void FillRectangleReal(int index, int offset)
    {
      _simpleShader.Uniform4("fragmentColor", _fillRectangleColors[index]);
      _simpleShader.DrawArrays(BeginMode.Polygon, offset, 4);
    }

    private void DrawImageReal(int index, int offset)
    {
      _drawImageTextures[index].Bind();
      _textureShader.Uniform1("Texture", _drawImageTextures[index].GlHandle);
      _textureShader.DrawArrays(BeginMode.Polygon, offset, 4);
      if (_drawImageTextures[index].DisposeAfterFirstUse)
        _drawImageTextures[index].Dispose();
    }

    private void FillEllipseReal(int index)
    {
      List<float> pos = new List<float>();
      float xc = _fillEllipseData[index].X + _fillEllipseData[index].Width / 2;
      float yc = _fillEllipseData[index].Y + _fillEllipseData[index].Height / 2;
      float a = _fillEllipseData[index].Width / 2.0f;
      float b = _fillEllipseData[index].Height / 2.0f;
      for (int i = 0; i <= 360; i++)
      {
        pos.Add(xc + (float)Math.Cos(i * Math.PI / 180) * a);
        pos.Add(yc + (float)Math.Sin(i * Math.PI / 180) * b);
      }
      _ellipseShader.Uniform4("fragmentColor",
        new Vector4(_fillEllipseData[index].EllipseBrush.Color.R * ColorFloat, _fillEllipseData[index].EllipseBrush.Color.G * ColorFloat, _fillEllipseData[index].EllipseBrush.Color.B * ColorFloat, _fillEllipseData[index].EllipseBrush.Color.A * ColorFloat));
      _ellipseShader.ChangeData(VBOdata.Positions, pos.ToArray());
      _ellipseShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _ellipseShader.DrawArrays(BeginMode.Polygon, 0, pos.Count / 2);
    }

    private void DrawEllipseReal(int index)
    {
      //It's not looks grate, but it fix memory leak
      List<float> pos1 = new List<float>();
      List<float> pos2 = new List<float>();
      List<float> pos3 = new List<float>();
      List<float> pos4 = new List<float>();

      float xc = _drawEllipseData[index].X + _drawEllipseData[index].Width / 2;
      float yc = _drawEllipseData[index].Y + _drawEllipseData[index].Height / 2;
      float penW = _drawEllipseData[index].EllipsePen.Width - 1;
      #region Big ellipse calculations

      float widthMax = _drawEllipseData[index].Width + penW;
      float heightMax = _drawEllipseData[index].Height + penW;

      float aMax = (widthMax * widthMax) / 4;
      float bMax = (heightMax * heightMax) / 4;
      float rMax = aMax * bMax;

      float xMax = -widthMax / 2;
      float yMax = 0;

      #endregion
      #region Small ellipse calculations

      float widthMin = _drawEllipseData[index].Width - (penW);
      float heightMin = _drawEllipseData[index].Height - (penW);

      float aMin = (widthMin * widthMin) / 4;
      float bMin = (heightMin * heightMin) / 4;
      float rMin = aMin * bMin;

      float xMin = -widthMin / 2;
      float yMin = 0;

      #endregion

      #region Ellipse coords calculation
      while (xMax <= 1)
      {
        //Третья четверть
        pos3.Add(xMax + xc);
        pos3.Add(yMax + yc);

        //Четвёртая четверть
        pos4.Add(yc + yMax);
        pos4.Add(xc - xMax);

        //Вторая четверть
        pos2.Add(yc - yMax);
        pos2.Add(xMax + xc);

        //Первая четверть
        pos1.Add(xc - xMax);
        pos1.Add(yc - yMax);

        #region Big ellipse
        float xMaxD = xMax + 1;
        float yMaxD = yMax;
        float xMaxS = xMax;
        float yMaxS = yMax + 1;
        if (bMax * (xMaxS * xMaxS + xMaxD * xMaxD) + aMax * (yMaxS * yMaxS + yMaxD * yMaxD) - 2 * rMax > 0)
        {
          xMax = xMaxD;
          yMax = yMaxD;
        }
        else
        {
          xMax = xMaxS;
          yMax = yMaxS;
        }
        #endregion

        if (Math.Abs(penW - 0) > 0.000001)
        {
          //Третья четверть
          pos3.Add(xMin + xc);
          pos3.Add(yMin + yc);
          //Четвёртая четверть
          pos4.Add(yc + yMin);
          pos4.Add(xc - xMin);
          //Вторая четверть
          pos2.Add(yc - yMin);
          pos2.Add(xMin + xc);
          //Первая четверть
          pos1.Add(xc - xMin);
          pos1.Add(yc - yMin);

          #region Small ellipse
          if (xMin <= 0)
          {
            float xMinD = xMin + 1;
            float yMinD = yMin;
            float xMinS = xMin;
            float yMinS = yMin + 1;
            if (bMin * (xMinS * xMinS + xMinD * xMinD) + aMin * (yMinS * yMinS + yMinD * yMinD) - 2 * rMin > 0)
            {
              xMin = xMinD;
              yMin = yMinD;
            }
            else
            {
              xMin = xMinS;
              yMin = yMinS;
            }
          }
          #endregion
        }
      }
      #endregion
      pos4.Reverse();
      pos2.Reverse();
      _ellipseShader.Uniform4("fragmentColor", new Vector4(_drawEllipseData[index].EllipsePen.Color.R * ColorFloat, _drawEllipseData[index].EllipsePen.Color.G * ColorFloat, _drawEllipseData[index].EllipsePen.Color.B * ColorFloat, _drawEllipseData[index].EllipsePen.Color.A * ColorFloat));
      _ellipseShader.Resize(VBOdata.Positions, pos1.Count * 4 * sizeof(float));
      _ellipseShader.ChangeData(VBOdata.Positions, pos1.ToArray());

      _ellipseShader.ChangeData(VBOdata.Positions, pos2.ToArray(), pos1.Count * sizeof(float));
      _ellipseShader.ChangeData(VBOdata.Positions, pos3.ToArray(), pos1.Count * sizeof(float) * 2);
      _ellipseShader.ChangeData(VBOdata.Positions, pos4.ToArray(), pos1.Count * sizeof(float) * 3);
      _ellipseShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _ellipseShader.DrawArrays(
        Math.Abs(penW - 0.0) < 0.000001
        ? BeginMode.Points
        : BeginMode.LineStrip, 0, pos1.Count * 2);
    }
    #endregion

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
      _ellipseShader.UniformMatrix4("projectionMatrix", projectionMatrix, true);
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
        _clipAreaRects.Add(value);
        _actions.Add(DrawActions.ClipArea);
        /*GL.Enable(EnableCap.ScissorTest);
        GL.Scissor(value.X, _windowSize.Height - value.Height - value.Y, value.Width, value.Height);*/
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
      _actions.Add(DrawActions.FillEllipse);
      _fillEllipseData.Add(new FillEllipseData { EllipseBrush = brush, X = x, Y = y, Width = width, Height = height });
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
      //using (Texture image = new Texture(tmp))
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
      DrawTexture(tmp, x, y, width, height);
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
      _actions.Add(DrawActions.DrawEllipse);
      _drawEllipseData.Add(new DrawEllipseData { EllipsePen = pen, X = x, Y = y, Width = width, Height = height });
    }

    /// <summary>
    /// Renders this instance.
    /// </summary>
    public void Render()
    {
      //GL.Finish();
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

      #endregion

      #region Offsets and drawing methods indexes initializers
      int vboOffset = 0;
      int textureVBOoffset = 0;
      int clipAreaNumber = 0;
      int fillRectangleNumber = 0;
      int fillEllipseNumber = 0;
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
            DrawEllipseReal(drawEllipseNumber);
            drawEllipseNumber++;
            break;
          case DrawActions.FillRectangle:
            FillRectangleReal(fillRectangleNumber, vboOffset);
            vboOffset += 4;
            fillRectangleNumber++;
            break;
          case DrawActions.FillEllipse:
            FillEllipseReal(fillEllipseNumber);
            fillEllipseNumber++;
            break;
          case DrawActions.ClipArea:
            GL.Scissor(_clipAreaRects[clipAreaNumber].X, _windowSize.Height - _clipAreaRects[clipAreaNumber].Height - _clipAreaRects[clipAreaNumber].Y, _clipAreaRects[clipAreaNumber].Width, _clipAreaRects[clipAreaNumber].Height);
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
      /*_fillRectangleColors.Clear();
      _vertexCoords.Clear();
      _drawImageTextures.Clear();
      _drawImageCoords.Clear();
      _drawImageTextureCoords.Clear();
      _drawEllipseData.Clear();
      _fillEllipseData.Clear();
      _actions.Clear();
      _drawLinesPens.Clear();*/
      _fillRectangleColors = new List<Vector4>();
      _vertexCoords = new List<float>();
      _drawImageTextures = new List<Texture>();
      _drawImageCoords = new List<float>();
      _drawImageTextureCoords = new List<float>();
      _drawEllipseData = new List<DrawEllipseData>();
      _fillEllipseData = new List<FillEllipseData>();
      _actions = new List<DrawActions>();
      _drawLinesPens = new List<Pen>();
      _clipAreaRects = new List<Rectangle>();
      GC.Collect();
      #endregion

      GL.Disable(EnableCap.Blend);
      GL.Disable(EnableCap.Texture2D);
      GL.Disable(EnableCap.ScissorTest);

      //GL.Finish();
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

}
