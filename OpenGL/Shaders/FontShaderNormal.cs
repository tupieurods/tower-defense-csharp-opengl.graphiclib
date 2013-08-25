using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL.Shaders
{
  internal class FontShaderNormal: FontShader
  {
    internal FontShaderNormal()
    {
      byte[] shaderByteSource = Properties.Resources.FontVertex;
      string shaderStr = shaderByteSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      if(!AddShader(ShaderType.VertexShader, shaderStr))
      {
        throw new Exception("Cannot create vertex shader");
      }
      shaderByteSource = Properties.Resources.FontFragmentSimple;
      shaderStr = shaderByteSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      if(!AddShader(ShaderType.FragmentShader, shaderStr))
      {
        throw new Exception("Cannot create fragment shader");
      }
      LinkShaderProgram();
      Vao.Resize(BufferTarget.ArrayBuffer, 10000);
      Uniform1("Texture", 4);
    }

    public override void Bind()
    {
      _fontTextures[CurrentTask].Bind(TextureUnit.Texture4);
    }
  }
}