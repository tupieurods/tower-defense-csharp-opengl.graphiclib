using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphicLib.OpenGL.Fonts;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL.Shaders
{
  class FontShader: ShaderProgram
  {
    public FontShader()
    {
      byte[] shaderByteSource = Properties.Resources.SimpleTextureVertex;
      string shaderStr = shaderByteSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      if(!AddShader(ShaderType.VertexShader, shaderStr))
      {
        throw new Exception("Cannot create vertex shader");
      }
      shaderByteSource = Properties.Resources.SimpleTextureFragment;
      shaderStr = shaderByteSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      if(!AddShader(ShaderType.FragmentShader, shaderStr))
      {
        throw new Exception("Cannot create fragment shader");
      }
      LinkShaderProgram();
      this.Vao.Resize(BufferTarget.ArrayBuffer, 10000);
      this.Uniform1("Texture", 3);
    }

    public void AddVertex(float x, float y, float texCoord1, float texCoord2)
    {
      Verticies.Add(x);
      Verticies.Add(y);
      Verticies.Add(texCoord1);
      Verticies.Add(texCoord2);
    }

    public void Bind()
    {
      FontManager.FontTexture.Bind(TextureUnit.Texture3);
    }

    #region Overrides of ShaderProgram

    /// <summary>
    /// Uploads the data to GPU.
    /// </summary>
    public override void UploadDataToGPU()
    {
      Vao.ChangeData(BufferTarget.ArrayBuffer, Verticies.ToArray());
      this.ChangeAttribute(BufferTarget.ArrayBuffer, "position", 2,
                           VertexAttribPointerType.Float, true, 4 * sizeof(float), 0);
      this.ChangeAttribute(BufferTarget.ArrayBuffer, "texcoord", 2,
                           VertexAttribPointerType.Float, true, 4 * sizeof(float), 2 * sizeof(float));
    }

    /// <summary>
    /// Draws next primitive
    /// </summary>
    public override void DrawNext()
    {
      this.DrawArrays(BeginMode.Triangles, 6 * CurrentTask, 6);
      CurrentTask++;
    }

    #endregion
  }
}
