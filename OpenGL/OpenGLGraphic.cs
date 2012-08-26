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

    private readonly ShaderProgram _textureShader;

    private const float ColorFloat = 1.0f / 255.0f;

    private readonly List<float> _fillRectangleCoords = new List<float>();
    private readonly List<Vector4> _fillRectangleColors = new List<Vector4>();

    private readonly List<float> _drawImageCoords = new List<float>();
    private readonly List<float> _drawImageTextureCoords = new List<float>();
    private readonly List<Texture> _drawImageTextures = new List<Texture>();

    private List<float> DrawLinesCoords = new List<float>();
    private List<Pen> DrawLinesPens = new List<Pen>();

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
      _fillRectangleCoords.Add(x); _fillRectangleCoords.Add(y);
      _fillRectangleCoords.Add(x + width); _fillRectangleCoords.Add(y);
      _fillRectangleCoords.Add(x + width); _fillRectangleCoords.Add(y + height);
      _fillRectangleCoords.Add(x); _fillRectangleCoords.Add(y + height);
      _fillRectangleColors.Add(new Vector4(brush.Color.R * ColorFloat, brush.Color.G * ColorFloat, brush.Color.B * ColorFloat, brush.Color.A * ColorFloat));

      /*_positions[0] = x; _positions[1] = y;
      _positions[2] = x + width; _positions[3] = y;
      _positions[4] = x + width; _positions[5] = y + height;
      _positions[6] = x; _positions[7] = y + height;*/
      /*_simpleShader.Uniform3("fragmentColor", new Vector3(brush.Color.R * ColorFloat, brush.Color.G * ColorFloat, brush.Color.B * ColorFloat));
      _simpleShader.ChangeData(VBOdata.Positions, _positions);
      _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _simpleShader.DrawArrays(BeginMode.Polygon, 0, 4);*/
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
      float a = width / 2.0f;
      float b = height / 2.0f;
      for (int i = 0; i <= 360; i++)
      {
        pos.Add(xc + (float)Math.Cos(i * Math.PI / 180) * a);
        pos.Add(yc + (float)Math.Sin(i * Math.PI / 180) * b);
      }
      //_simpleShader.Uniform3("fragmentColor", new Vector3(brush.Color.R * ColorFloat, brush.Color.G * ColorFloat, brush.Color.B * ColorFloat));
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
      Size textSize = TextRenderer.MeasureText(s, font);
      Bitmap tmp = new Bitmap(textSize.Width, textSize.Height);
      var canva = Graphics.FromImage(tmp);
      canva.Clear(Color.Transparent);
      canva.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
      canva.DrawString(s, font, brush, 0, 0);
      //using (Texture image = new Texture(tmp))
      DrawImageReal(new Texture(tmp, true), (int)point.X, (int)point.Y, tmp.Width, tmp.Height);
    }

    /// <summary>
    /// Draws Texture
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    private void DrawImageReal(Texture image, int x, int y, int width, int height)
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
      /*GL.Enable(EnableCap.Texture2D);
      GL.Enable(EnableCap.Blend);
      GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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
      image.Bind();
      _textureShader.Uniform1("Texture", image.GlHandle);
      _textureShader.DrawArrays(BeginMode.Polygon, 0, 4);
      GL.Disable(EnableCap.Blend);
      GL.Disable(EnableCap.Texture2D);*/
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
      DrawImageReal(tmp, x, y, width, height);
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
      DrawLinesCoords.Add(x1); DrawLinesCoords.Add(y1);
      DrawLinesCoords.Add(x2); DrawLinesCoords.Add(y2);
      DrawLinesPens.Add(pen);
      /*_positions[0] = x1; _positions[1] = y1;
      _positions[2] = x2; _positions[3] = y2;*/
      //_simpleShader.Uniform3("fragmentColor", new Vector3(pen.Color.R * ColorFloat, pen.Color.G * ColorFloat, pen.Color.B * ColorFloat));
      /*_simpleShader.ChangeData(VBOdata.Positions, _positions);
      _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      GL.LineWidth(pen.Width);
      _simpleShader.DrawArrays(BeginMode.Lines, 0, 2);*/
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
      List<float> pos1 = new List<float>();
      List<float> pos2 = new List<float>();
      List<float> pos3 = new List<float>();
      List<float> pos4 = new List<float>();

      float xc = x + width / 2;
      float yc = y + height / 2;
      float penW = pen.Width - 1;
      #region Big ellipse calculations

      float widthMax = width + penW;
      float heightMax = height + penW;

      float aMax = (widthMax * widthMax) / 4;
      float bMax = (heightMax * heightMax) / 4;
      float rMax = aMax * bMax;

      float xMax = -widthMax / 2;
      float yMax = 0;

      #endregion
      #region Small ellipse calculations

      float widthMin = width - (penW);
      float heightMin = height - (penW);

      float aMin = (widthMin * widthMin) / 4;
      float bMin = (heightMin * heightMin) / 4;
      float rMin = aMin * bMin;

      float xMin = -widthMin / 2;
      float yMin = 0;

      #endregion

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
      pos4.Reverse();
      pos2.Reverse();
      //_simpleShader.Uniform3("fragmentColor", new Vector3(pen.Color.R * ColorFloat, pen.Color.G * ColorFloat, pen.Color.B * ColorFloat));
      _simpleShader.Resize(VBOdata.Positions, pos1.Count * 4 * sizeof(float));
      _simpleShader.ChangeData(VBOdata.Positions, pos1.ToArray());

      _simpleShader.ChangeData(VBOdata.Positions, pos2.ToArray(), pos1.Count * sizeof(float));
      _simpleShader.ChangeData(VBOdata.Positions, pos3.ToArray(), pos1.Count * sizeof(float) * 2);
      _simpleShader.ChangeData(VBOdata.Positions, pos4.ToArray(), pos1.Count * sizeof(float) * 3);
      _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
      _simpleShader.DrawArrays(
        Math.Abs(penW - 0.0) < 0.000001
        ? BeginMode.Points
        : BeginMode.LineStrip, 0, pos1.Count * 2);
    }

    /// <summary>
    /// Renders this instance.
    /// </summary>
    public void Render()
    {
      if (_fillRectangleColors.Count != 0)
      {
        _simpleShader.ChangeData(VBOdata.Positions, _fillRectangleCoords.ToArray());
        _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        for (int i = 0; i < _fillRectangleColors.Count; i++)
        {
          _simpleShader.Uniform4("fragmentColor", _fillRectangleColors[i]);
          _simpleShader.DrawArrays(BeginMode.Polygon, i * 4, 4);
        }
        _fillRectangleColors.Clear();
        _fillRectangleCoords.Clear();
      }
      if (_drawImageTextures.Count != 0)
      {
        GL.Enable(EnableCap.Texture2D);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        _textureShader.ChangeData(VBOdata.Positions, _drawImageCoords.ToArray());
        _textureShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        _textureShader.ChangeData(VBOdata.TextureCoord, _drawImageTextureCoords.ToArray());
        _textureShader.ChangeAttribute(VBOdata.TextureCoord, "texcoord", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        for (int i = 0; i < _drawImageTextures.Count; i++)
        {
          _drawImageTextures[i].Bind();
          _textureShader.Uniform1("Texture", _drawImageTextures[i].GlHandle);
          _textureShader.DrawArrays(BeginMode.Polygon, i * 4, 4);
          if (_drawImageTextures[i].DisposeAfterFirstUse)
            _drawImageTextures[i].Dispose();
        }
        GL.Disable(EnableCap.Blend);
        GL.Disable(EnableCap.Texture2D);
        _drawImageTextures.Clear();
        _drawImageCoords.Clear();
        _drawImageTextureCoords.Clear();
      }
      if (DrawLinesPens.Count != 0)
      {
        _simpleShader.ChangeData(VBOdata.Positions, DrawLinesCoords.ToArray());
        _simpleShader.ChangeAttribute(VBOdata.Positions, "position", 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        for (int i = 0; i < DrawLinesPens.Count; i++)
        {
          _simpleShader.Uniform4("fragmentColor", new Vector4(DrawLinesPens[i].Color.R * ColorFloat, DrawLinesPens[i].Color.G * ColorFloat, DrawLinesPens[i].Color.B * ColorFloat, DrawLinesPens[i].Color.A * ColorFloat));
          GL.LineWidth(DrawLinesPens[i].Width);
          _simpleShader.DrawArrays(BeginMode.Lines, i * 2, 2);
        }
        DrawLinesPens.Clear();
        DrawLinesCoords.Clear();
      }
      //GL.Finish();
      GC.Collect();
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
