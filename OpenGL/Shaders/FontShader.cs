using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GraphicLib.OpenGL.Fonts;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL.Shaders
{
  internal abstract class FontShader: ShaderProgram
  {
    private readonly List<int> _stringLens = new List<int>();
    private readonly List<Color> _colors = new List<Color>();
    protected readonly List<Texture> _fontTextures = new List<Texture>();
    private int _trianglesOffset;

    /// <summary>
    /// constant for color convertion
    /// </summary>
    private const float ColorFloat = 1.0f / 255.0f;

    private void AddVertex(float x, float y, float texCoord1, float texCoord2)
    {
      Verticies.Add(x);
      Verticies.Add(y);
      Verticies.Add(texCoord1);
      Verticies.Add(texCoord2);
    }

    public void AddTask(string s, Font font, Brush brush, PointF point)
    {
      if(!(brush is SolidBrush))
      {
        throw new NotSupportedException("brush");
      }
      _colors.Add((brush as SolidBrush).Color);
      MyFont myFont = FontManager.GetFont(FontManager.FindClosestFont(font));
      string[] lines = s.Split('\n');
      float k = (font.SizeInPoints / myFont.FontInfo.Pt);
      //k = 1;
      int glyphHeight = (int)(k * myFont.FontInfo.Height);
      int realStrLen = 0;
      foreach(string str in lines)
      {
        int strLen = str.Length;
        realStrLen += strLen;
        float x = point.X;
        for(int j = 0; j < strLen; j++)
        {
          if(!myFont.Symbols.ContainsKey(str[j]))
          {
            realStrLen--;
            continue;
          }
          GlyphData symbol = myFont.Symbols[str[j]];
          AddVertex(x + symbol.XOffset * k,
                    point.Y + symbol.YOffset * k,
                    symbol.TextureXPos,
                    symbol.TextureYPos);
          AddVertex(x + (symbol.XOffset + symbol.Width) * k,
                    point.Y + symbol.YOffset * k,
                    symbol.TextureWidth,
                    symbol.TextureYPos);
          AddVertex(x + symbol.XOffset * k,
                    point.Y + (symbol.YOffset + symbol.Height) * k,
                    symbol.TextureXPos,
                    symbol.TextureHeight);

          AddVertex(x + (symbol.XOffset + symbol.Width) * k,
                    point.Y + symbol.YOffset * k,
                    symbol.TextureWidth,
                    symbol.TextureYPos);
          AddVertex(x + (symbol.XOffset + symbol.Width) * k,
                    point.Y + (symbol.YOffset + symbol.Height) * k,
                    symbol.TextureWidth,
                    symbol.TextureHeight);
          AddVertex(x + symbol.XOffset * k,
                    point.Y + (symbol.YOffset + symbol.Height) * k,
                    symbol.TextureXPos,
                    symbol.TextureHeight);
          x += (myFont.Symbols[str[j]].OrigW + (j != strLen - 1 ? myFont.GetKerningDelta(str[j], str[j + 1]) : 0.0f)) * k;
        }
        point.Y += glyphHeight;
      }
      _stringLens.Add(realStrLen);
      _fontTextures.Add(myFont.FontTexture);
    }

    public virtual void Bind()
    {
      _fontTextures[CurrentTask].Bind(TextureUnit.Texture3);
    }

    public new void Clear()
    {
      base.Clear();
      _trianglesOffset = 0;
      _stringLens.Clear();
      _colors.Clear();
      _fontTextures.Clear();
    }

    #region Overrides of ShaderProgram

    /// <summary>
    /// Uploads the data to GPU.
    /// </summary>
    public override void UploadDataToGPU()
    {
      Vao.ChangeData(BufferTarget.ArrayBuffer, Verticies.ToArray());
      ChangeAttribute(BufferTarget.ArrayBuffer, "position", 2,
                           VertexAttribPointerType.Float, true, 4 * sizeof(float), 0);
      ChangeAttribute(BufferTarget.ArrayBuffer, "texcoord", 2,
                           VertexAttribPointerType.Float, true, 4 * sizeof(float), 2 * sizeof(float));
    }

    /// <summary>
    /// Draws next primitive
    /// </summary>
    public override void DrawNext()
    {
      Bind();
      Uniform4("inColor", _colors[CurrentTask].R * ColorFloat, _colors[CurrentTask].G * ColorFloat,
               _colors[CurrentTask].B * ColorFloat, _colors[CurrentTask].A * ColorFloat);
      DrawArrays(BeginMode.Triangles, _trianglesOffset, _stringLens[CurrentTask] * 6);
      _trianglesOffset += _stringLens[CurrentTask] * 6;
      CurrentTask++;
    }

    #endregion
  }
}