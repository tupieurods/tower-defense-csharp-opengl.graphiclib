using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL
{
  public class ShaderProgram: IDisposable
  {
    /// <summary>
    /// Shader program id on GPU
    /// </summary>
    private readonly int _shaderProgram;

    /// <summary>
    /// VAO, which associated with shader program
    /// </summary>
    private VAO _vao;

    /// <summary>
    /// Shader attributes locations
    /// </summary>
    private readonly Dictionary<string, int> _attribLocations = new Dictionary<string, int>();

    /// <summary>
    /// Shader unifrom vars locations
    /// </summary>
    private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderProgram"/> class.
    /// </summary>
    /// <param name="vertexShaderSource">The vertex shader source.</param>
    /// <param name="fragmentShaderSource">The fragment shader source.</param>
    /// <param name="vao">The vao.</param>
    public ShaderProgram(byte[] vertexShaderSource, byte[] fragmentShaderSource, VAO vao = null)
    {
      _vao = vao;
      //Вершинный шейдер
      int vertexShader = GL.CreateShader(ShaderType.VertexShader);
      string shaderStr = vertexShaderSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      GL.ShaderSource(vertexShader, shaderStr);
      //Фрагментный шейдер
      int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
      shaderStr = fragmentShaderSource.Aggregate("", (current, t) => current + Convert.ToChar(t));
      GL.ShaderSource(fragmentShader, shaderStr);
      //Компиляция
      GL.CompileShader(vertexShader);
      int status;
      GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
      if(status != 1)
      {
        throw new ArgumentException("vertexShaderSource");
      }
      GL.CompileShader(fragmentShader);
      GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
      if(status != 1)
      {
        throw new ArgumentException("fragmentShaderSource");
      }
      //Сборка шейдерной программы
      _shaderProgram = GL.CreateProgram();
      GL.AttachShader(_shaderProgram, vertexShader);
      GL.AttachShader(_shaderProgram, fragmentShader);
      GL.LinkProgram(_shaderProgram);
    }

    /// <summary>
    /// Attaches the VAO.
    /// </summary>
    /// <param name="vao">The vao.</param>
    public void AttachVAO(VAO vao)
    {
      _vao = vao;
    }

    /// <summary>
    /// Changes the attribute.
    /// </summary>
    /// <param name="VBOtype">The VBO type.</param>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <param name="size">The size.</param>
    /// <param name="type">The type.</param>
    /// <param name="normalized">if set to <c>true</c> [normalized].</param>
    /// <param name="stride">The stride.</param>
    /// <param name="offset">The offset.</param>
    public void ChangeAttribute(VBOdata VBOtype, string attributeName, int size, VertexAttribPointerType type,
                                bool normalized, int stride, int offset)
    {
      _vao.BindWithVBO(VBOtype);

      if(!_attribLocations.ContainsKey(attributeName))
      {
        _attribLocations.Add(attributeName, GL.GetAttribLocation(_shaderProgram, attributeName));
      }
      int attributeLocation = _attribLocations[attributeName];
      if(attributeLocation != -1)
      {
        GL.VertexAttribPointer(attributeLocation, size, type, normalized, stride, offset);
        GL.EnableVertexAttribArray(attributeLocation);
      }
      else
      {
        throw new ShaderParametrNotFoundException();
      }
      _vao.UnBindWithVBO(VBOtype);
    }

    public void Resize(VBOdata VBOtype, int size)
    {
      _vao.Resize(VBOtype, size);
    }

    /// <summary>
    /// Changes the data in VAO
    /// </summary>
    /// <param name="VBOtype">The VBO type.</param>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns></returns>
    public bool ChangeData(VBOdata VBOtype, float[] buffer, int offset = 0)
    {
      bool result = _vao.ChangeData(VBOtype, buffer, offset);
      return result;
    }

    /// <summary>
    /// Changes the data in VAO
    /// </summary>
    /// <param name="VBOtype">The VBO type.</param>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns></returns>
    public bool ChangeData(VBOdata VBOtype, float[,] buffer, int offset = 0)
    {
      bool result = _vao.ChangeData(VBOtype, buffer, offset);
      return result;
    }

    //ForDebugOnly
    public bool ChangeData(VBOdata VBOtype, double[] buffer, int offset = 0)
    {
      bool result = _vao.ChangeData(VBOtype, buffer, offset);
      return result;
    }

    /// <summary>
    /// Changes the data in VAO
    /// </summary>
    /// <param name="VBOtype">The VBO type.</param>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns></returns>
    public bool ChangeData(VBOdata VBOtype, int[] buffer, int offset = 0)
    {
      bool result = _vao.ChangeData(VBOtype, buffer, offset);
      return result;
    }

    /// <summary>
    /// Changes the data in VAO
    /// </summary>
    /// <param name="VBOtype">The VBO type.</param>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns></returns>
    public bool ChangeData(VBOdata VBOtype, uint[] buffer, int offset = 0)
    {
      bool result = _vao.ChangeData(VBOtype, buffer, offset);
      return result;
    }

    /// <summary>
    /// Sets this shader program as active on GPU
    /// </summary>
    public void UseProgram()
    {
      GL.UseProgram(_shaderProgram);
    }

    /// <summary>
    /// Sets this shader program as inactive on GPU
    /// </summary>
    public static void StopUseProgram()
    {
      GL.UseProgram(0);
    }

    /// <summary>
    /// Uniforms the matrix4.
    /// </summary>
    /// <param name="matrixName">Name of the matrix.</param>
    /// <param name="matrix">The matrix.</param>
    /// <param name="transpose">if set to <c>true</c> [transpose].</param>
    public void UniformMatrix4(string matrixName, float[] matrix, bool transpose)
    {
      UseProgram();
      int projectionMatrixLocation = GL.GetUniformLocation(_shaderProgram, matrixName);
      if(projectionMatrixLocation != -1)
      {
        GL.UniformMatrix4(projectionMatrixLocation, 1, transpose, matrix);
      }
      else
      {
        throw new ShaderParametrNotFoundException();
      }
      StopUseProgram();
    }

    public void UniformMatrix4(string matrixName, Matrix4 matrix, bool transpose)
    {
      UseProgram();
      int projectionMatrixLocation = GL.GetUniformLocation(_shaderProgram, matrixName);
      if(projectionMatrixLocation != -1)
      {
        GL.UniformMatrix4(projectionMatrixLocation, transpose, ref matrix);
      }
      else
      {
        throw new ShaderParametrNotFoundException();
      }
      StopUseProgram();
    }

    #region Uniform1234

    private int GetUniformLocation(string parametrName)
    {
      if(!_uniformLocations.ContainsKey(parametrName))
      {
        _uniformLocations.Add(parametrName, GL.GetUniformLocation(_shaderProgram, parametrName));
        if(_uniformLocations[parametrName] == -1)
        {
          throw new ShaderParametrNotFoundException();
        }
      }
      return _uniformLocations[parametrName];
    }

    public void Uniform1(string parametrName, float value)
    {
      UseProgram();
      GL.Uniform1(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    //ForDebugOnly
    public void Uniform1(string parametrName, double value)
    {
      UseProgram();
      GL.Uniform1(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    public void Uniform2(string parametrName, Vector2 value)
    {
      UseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    public void Uniform3(string parametrName, Vector3 value)
    {
      UseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    public void Uniform3(string parametrName, int count, float[] value)
    {
      UseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    public void Uniform4(string parametrName, Vector4 value)
    {
      UseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    public void Uniform4(string parametrName, float v0, float v1, float v2, float v3)
    {
      UseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), v0, v1, v2, v3);
      StopUseProgram();
    }

    #endregion

    /// <summary>
    /// Draws the arrays.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="first">The first.</param>
    /// <param name="count">The count.</param>
    public void DrawArrays(BeginMode mode, int first, int count)
    {
      UseProgram();
      _vao.Bind();
      GL.DrawArrays(mode, first, count);
      _vao.UnBind();
      StopUseProgram();
    }

    /// <summary>
    /// Draws the elements.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="count">The count.</param>
    /// <param name="type">The type.</param>
    public void DrawElements(BeginMode mode, int count, DrawElementsType type = DrawElementsType.UnsignedInt)
    {
      if(!_vao.VBOexists(VBOdata.Index))
      {
        throw new VBONotFoundException("Index VBO not found");
      }
      UseProgram();
      _vao.BindWithVBO(VBOdata.Index);
      GL.DrawElements(mode, count, type, IntPtr.Zero);
      _vao.UnBindWithVBO(VBOdata.Index);
      StopUseProgram();
    }

    #region Implementation of IDisposable

    /// <summary>
    /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
      _vao.Dispose();
      GL.DeleteProgram(_shaderProgram);
    }

    #endregion
  }
}