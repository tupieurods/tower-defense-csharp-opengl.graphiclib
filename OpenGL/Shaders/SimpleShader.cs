using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL
{
  public sealed class SimpleShader: ShaderProgram
  {
    private Vector4 _colorVector;

    /// <summary>
    /// constant for color convertion
    /// </summary>
    private const float ColorFloat = 1.0f / 255.0f;

    public SimpleShader()
    {
      byte[] shaderByteSource = Properties.Resources.SimpleVertexShader;
      string shaderStr = shaderByteSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      if(!AddShader(ShaderType.VertexShader, shaderStr))
      {
        throw new Exception("Cannot create vertex shader");
      }
      shaderByteSource = Properties.Resources.SimpleFragmentShader;
      shaderStr = shaderByteSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      if(!AddShader(ShaderType.FragmentShader, shaderStr))
      {
        throw new Exception("Cannot create fragment shader");
      }
      LinkShaderProgram();
      this.Vao.Resize(BufferTarget.ArrayBuffer, 1000);
    }

    public void AddTask(Color color, IList<float> positions)
    {
      if(positions.Count != 8)
      {
        throw new ArgumentException("Positions array should have 8 elements");
      }
      _colorVector.X = color.R * ColorFloat;
      _colorVector.Y = color.G * ColorFloat;
      _colorVector.Z = color.B * ColorFloat;
      _colorVector.W = color.A * ColorFloat;
      for(int i = 0; i < 4; i++)
      {
        Verticies.Add(positions[i * 2]);
        Verticies.Add(positions[i * 2 + 1]);
        Verticies.Add(_colorVector.X);
        Verticies.Add(_colorVector.Y);
        Verticies.Add(_colorVector.Z);
        Verticies.Add(_colorVector.W);
      }
    }

    #region Overrides of ShaderProgram

    public override void UploadDataToGPU()
    {
      Vao.ChangeData(BufferTarget.ArrayBuffer, Verticies.ToArray());
      this.ChangeAttribute(BufferTarget.ArrayBuffer, "position", 2,
                           VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
      this.ChangeAttribute(BufferTarget.ArrayBuffer, "vertexColor", 4,
                           VertexAttribPointerType.Float, false, 6 * sizeof(float), 2 * sizeof(float));
    }

    public override void DrawNext()
    {
      this.DrawArrays(BeginMode.Polygon, CurrentTask * 4, 4);
      CurrentTask++;
    }

    #endregion
  }
}