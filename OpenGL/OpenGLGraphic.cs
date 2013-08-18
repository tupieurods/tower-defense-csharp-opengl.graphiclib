#define debug

using System;
using System.Collections.Generic;
using System.Drawing;
using GraphicLib.Interfaces;
using GraphicLib.OpenGL.Fonts;
using GraphicLib.OpenGL.Shaders;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace GraphicLib.OpenGL
{
  /// <summary>
  /// Implements IGraphic for VideoCard with OpenGL
  /// </summary>
  public class OpenGLGraphic: IGraphic
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
    /// Simple shader for drawing polygons
    /// </summary>
    private readonly SimpleShader _polygonShader;

    /// <summary>
    /// Simple shader for drawing textures
    /// </summary>
    private readonly TextureShader _textureShader;

    private readonly FontShader _fontShader;

    /// <summary>
    /// Shader for ellipse drawing and filling
    /// </summary>
    private readonly EllipseShader _ellipseShader;

    #region Order data

    /// <summary>
    /// Rendering order action
    /// </summary>
    private readonly List<DrawActions> _actions = new List<DrawActions>();

    

    /// <summary>
    /// Clipping area rectangles
    /// </summary>
    private readonly List<Rectangle> _clipAreaRects = new List<Rectangle>();

    /// <summary>
    /// For preventing recreating float[8] array
    /// </summary>
    private readonly float[] _vertexArr = new float[8];

    #endregion

    public OpenGLGraphic(Size windowSize)
    {
      FontManager.LoadFonts(Environment.CurrentDirectory + @"\Data\Fonts\");
      _polygonShader = new SimpleShader();
      _textureShader = new TextureShader();
      _ellipseShader = new EllipseShader();
      _fontShader = new FontShader();
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
      _textureShader.AddTexture(image);
      _textureShader.AddVertex(x, y, 0.0f, 0.0f);
      _textureShader.AddVertex(x + width, y, 1.0f, 0.0f);
      _textureShader.AddVertex(x + width, y + height, 1.0f, 1.0f);
      _textureShader.AddVertex(x, y + height, 0.0f, 1.0f);
      _actions.Add(DrawActions.DrawImage);
    }

    /// <summary>
    /// Adds the ellipse drawing task.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="border">The border.</param>
    /// <param name="color">The color.</param>
    private void AddEllipseDrawingTask(float x, float y, float width, float height, float border, Color color)
    {
      _ellipseShader.AddTask(x, y, width, height, border, color);
      _actions.Add(DrawActions.DrawEllipse);
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
      Matrix4 projectionMatrix =
        Matrix4.CreateOrthographicOffCenter(0, _windowSize.Width - 1, _windowSize.Height - 1, 0, -1, 1);
      _polygonShader.UniformMatrix4("projectionMatrix", projectionMatrix, false);
      _textureShader.UniformMatrix4("projectionMatrix", projectionMatrix, false);
      _ellipseShader.UniformMatrix4("projectionMatrix", projectionMatrix, false);
      _fontShader.UniformMatrix4("projectionMatrix", projectionMatrix, false);
      _ellipseShader.WindowHeight = _windowSize.Height;
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
      _vertexArr[0] = x;
      _vertexArr[1] = y;
      _vertexArr[2] = x + width;
      _vertexArr[3] = y;
      _vertexArr[4] = x + width;
      _vertexArr[5] = y + height;
      _vertexArr[6] = x;
      _vertexArr[7] = y + height;
      _polygonShader.AddTask(brush.Color, _vertexArr);
      _actions.Add(DrawActions.Polygon);
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
      _fontShader.AddTask(s, font, brush, point);
      
      _actions.Add(DrawActions.DrawString);
#if !debug
      Size textSize = TextRenderer.MeasureText(s, font);
      Bitmap tmp = new Bitmap(textSize.Width, textSize.Height);
      var canva = Graphics.FromImage(tmp);
      canva.Clear(Color.Transparent);
      canva.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
      canva.DrawString(s, font, brush, 0, 0);
      DrawTexture(new Texture(tmp, true), (int)point.X, (int)point.Y, tmp.Width, tmp.Height);
#endif
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
      if(img == null)
      {
        throw new ArgumentNullException("image");
      }
      int hashCode = img.GetHashCode();
      Texture tmp;
      if(!Cache.ContainsKey(hashCode))
      {
        tmp = new Texture(img);
        Cache.Add(hashCode, tmp);
      }
      else if(img.Tag != null && (int)img.Tag == 1)
      {
        Cache[hashCode].Dispose();
        tmp = new Texture(img);
        Cache[hashCode] = tmp;
      }
      else
      {
        tmp = Cache[hashCode];
      }
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
      _textureShader.AddTexture(tex);
      _textureShader.AddVertex(drawingRect.X, drawingRect.Y,
                               (float)imageRect.X / image.Width, (float)imageRect.Y / image.Height);

      _textureShader.AddVertex(drawingRect.X + drawingRect.Width, drawingRect.Y,
                               (float)imageRect.Width / image.Width, (float)imageRect.Y / image.Height);

      _textureShader.AddVertex(drawingRect.X + drawingRect.Width, drawingRect.Y + drawingRect.Height,
                               (float)imageRect.Width / image.Width, (float)imageRect.Height / image.Height);

      _textureShader.AddVertex(drawingRect.X, drawingRect.Y + drawingRect.Height,
                               (float)imageRect.X / image.Width, (float)imageRect.Height / image.Height);
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

    private static void Swap<T>(ref T left, ref T right)
    {
      T temp = left;
      left = right;
      right = temp;
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
      float d1 = (pen.Width - 1.0f) / 2.0f;
      float d2 = pen.Width - 1.0f - d1;
      if(y1 > y2)
      {
        Swap(ref y1, ref y2);
        Swap(ref x1, ref x2);
      }
      if(d1 <= 0.0f && d2 <= 0.0f)
      {
        d1 = 1.0f;
      }
      float vx = x2 - x1, vy = y2 - y1;
      float len = (float)Math.Sqrt(vx * vx + vy * vy);
      vx /= len;
      vy /= len;
      Swap(ref vx, ref vy);
      vx = -vx;
      _vertexArr[0] = x1 - d1 * vx;
      _vertexArr[1] = y1 - d1 * vy;
      _vertexArr[2] = x1 + d2 * vx;
      _vertexArr[3] = y1 + d2 * vy;
      _vertexArr[4] = x2 + d2 * vx;
      _vertexArr[5] = y2 + d2 * vy;
      _vertexArr[6] = x2 - d1 * vx;
      _vertexArr[7] = y2 - d1 * vy;
      _polygonShader.AddTask(pen.Color, _vertexArr);
      _actions.Add(DrawActions.Polygon);
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
      if(Math.Abs(pen.Width - Math.Min(width, height)) < 0.1)
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

      int clipAreaNumber = 0;
      _polygonShader.UploadDataToGPU();
      _textureShader.UploadDataToGPU();
      _ellipseShader.UploadDataToGPU();
      _fontShader.UploadDataToGPU();

      foreach(var drawAction in _actions)
      {
        switch(drawAction)
        {
          case DrawActions.Polygon:
            _polygonShader.DrawNext();
            break;
          case DrawActions.DrawImage:
            _textureShader.DrawNext();
            break;
          case DrawActions.DrawEllipse:
            _ellipseShader.DrawNext();
            break;
          case DrawActions.ClipArea:
            GL.Scissor(_clipAreaRects[clipAreaNumber].X,
                       _windowSize.Height - _clipAreaRects[clipAreaNumber].Height - _clipAreaRects[clipAreaNumber].Y,
                       _clipAreaRects[clipAreaNumber].Width,
                       _clipAreaRects[clipAreaNumber].Height);
            clipAreaNumber++;
            break;
          case DrawActions.DrawString:
            _fontShader.DrawNext();
            
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      #region data clearing

      _polygonShader.Clear();
      _textureShader.Clear();
      _ellipseShader.Clear();
      _fontShader.Clear();
      _actions.Clear();
      _clipAreaRects.Clear();

      #endregion

      GL.Disable(EnableCap.Blend);
      GL.Disable(EnableCap.Texture2D);
      GL.Disable(EnableCap.ScissorTest);
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void ClearCache()
    {
      if(Cache.Count == 0)
      {
        return;
      }
      Cache.Clear();
    }

    /// <summary>
    /// Removes the image from cache.
    /// </summary>
    public void RemoveImageFromCache(Image image)
    {
      if(image != null && Cache.ContainsKey(image.GetHashCode()))
      {
        Cache.Remove(image.GetHashCode());
      }
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