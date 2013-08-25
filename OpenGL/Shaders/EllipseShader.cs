using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL
{
  public sealed class EllipseShader: ShaderProgram
  {
    private readonly List<EllipseData> _uniformData = new List<EllipseData>();

    //To prevent array recreating
    private readonly float[] _ellipseConfiguration = new float[9];

    /// <summary>
    /// constant for color convertion
    /// </summary>
    private const float ColorFloat = 1.0f / 255.0f;

    public int WindowHeight { private get; set; }

    public EllipseShader()
    {
      byte[] shaderByteSource = Properties.Resources.SimpleVertexShader;
      string shaderStr = shaderByteSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      if(!AddShader(ShaderType.VertexShader, shaderStr))
      {
        throw new Exception("Cannot create vertex shader");
      }
      shaderByteSource = Properties.Resources.EllipseFragment;
      shaderStr = shaderByteSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      if(!AddShader(ShaderType.FragmentShader, shaderStr))
      {
        throw new Exception("Cannot create fragment shader");
      }
      LinkShaderProgram();
      this.Vao.Resize(BufferTarget.ArrayBuffer, 1000);
    }

    public void AddTask(float x, float y, float width, float height, float border, Color color)
    {
      float widthMax = width + border + 5.0f; //+5 for AA support
      float heightMax = height + border + 5.0f;
      float xc = x + width / 2.0f;
      float yc = y + height / 2.0f;
      Verticies.Add(xc - widthMax / 2.0f);
      Verticies.Add(yc - heightMax / 2.0f);
      Verticies.Add(xc + widthMax / 2.0f);
      Verticies.Add(yc - heightMax / 2.0f);
      Verticies.Add(xc + widthMax / 2.0f);
      Verticies.Add(yc + heightMax / 2.0f);
      Verticies.Add(xc - widthMax / 2.0f);
      Verticies.Add(yc + heightMax / 2.0f);
      _uniformData.Add(new EllipseData
                         {Color = color, Xc = xc, Yc = yc, Xr = width / 2.0f, Yr = height / 2.0f, Border = border});
    }

    public new void Clear()
    {
      base.Clear();
      _uniformData.Clear();
    }

    #region Overrides of ShaderProgram

    public override void UploadDataToGPU()
    {
      Vao.ChangeData(BufferTarget.ArrayBuffer, Verticies.ToArray());
      this.ChangeAttribute(BufferTarget.ArrayBuffer, "position", 2,
                           VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
    }

    public override void DrawNext()
    {
      _ellipseConfiguration[0] = _uniformData[CurrentTask].Color.R * ColorFloat;
      _ellipseConfiguration[1] = _uniformData[CurrentTask].Color.G * ColorFloat;
      _ellipseConfiguration[2] = _uniformData[CurrentTask].Color.B * ColorFloat;
      _ellipseConfiguration[3] = _uniformData[CurrentTask].Xc;
      _ellipseConfiguration[4] = WindowHeight - _uniformData[CurrentTask].Yc;
      _ellipseConfiguration[5] = 0.0f;
      _ellipseConfiguration[6] = _uniformData[CurrentTask].Xr;
      _ellipseConfiguration[7] = _uniformData[CurrentTask].Yr;
      _ellipseConfiguration[8] = _uniformData[CurrentTask].Border;
      this.Uniform3("conf", 3, _ellipseConfiguration);
      this.DrawArrays(BeginMode.Polygon, CurrentTask * 4, 4);
      CurrentTask++;
    }

    #endregion
  }
}