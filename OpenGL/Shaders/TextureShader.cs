using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL
{
  public sealed class TextureShader: ShaderProgram
  {
    private readonly List<Texture> _textures = new List<Texture>();

    public TextureShader()
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
      this.Vao.Resize(BufferTarget.ArrayBuffer, 1000);
      this.Uniform1("Texture", 1);
    }

    public void AddTexture(Texture texture)
    {
      _textures.Add(texture);
    }

    public void AddVertex(float x, float y, float texCoord1, float texCoord2)
    {
      Verticies.Add(x);
      Verticies.Add(y);
      Verticies.Add(texCoord1);
      Verticies.Add(texCoord2);
    }

    public new void Clear()
    {
      base.Clear();
      _textures.Clear();
    }

    #region Overrides of ShaderProgram

    public override void UploadDataToGPU()
    {
      Vao.ChangeData(BufferTarget.ArrayBuffer, Verticies.ToArray());
      this.ChangeAttribute(BufferTarget.ArrayBuffer, "position", 2,
                           VertexAttribPointerType.Float, true, 4 * sizeof(float), 0);
      this.ChangeAttribute(BufferTarget.ArrayBuffer, "texcoord", 2,
                           VertexAttribPointerType.Float, true, 4 * sizeof(float), 2 * sizeof(float));
    }

    public override void DrawNext()
    {
      _textures[CurrentTask].Bind(TextureUnit.Texture1);
      this.DrawArrays(BeginMode.Polygon, 4 * CurrentTask, 4);
      if(_textures[CurrentTask].DisposeAfterFirstUse)
      {
        _textures[CurrentTask].Dispose();
      }
      CurrentTask++;
    }

    #endregion
  }
}