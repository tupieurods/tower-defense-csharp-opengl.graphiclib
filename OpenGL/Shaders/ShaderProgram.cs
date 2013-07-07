using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL
{
  public abstract class ShaderProgram : IDisposable
  {
    /// <summary>
    /// Shader program id on GPU
    /// </summary>
    private readonly int _shaderProgramID;

    /// <summary>
    /// VAO, which associated with shader program
    /// </summary>
    protected readonly VAO Vao;

    /// <summary>
    /// Information about verticies. Should be uploaded to VBO
    /// </summary>
    protected readonly List<float> Verticies = new List<float>();

    /// <summary>
    /// Index of current primitive for drawing
    /// </summary>
    protected int CurrentTask;

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
    internal ShaderProgram()
    {
      Vao = new VAO();
      _shaderProgramID = GL.CreateProgram();
    }

    /// <summary>
    /// Adds the shader to shader program.
    /// </summary>
    /// <param name="shaderType">Type of the shader.</param>
    /// <param name="shaderSource">The shader source.</param>
    /// <returns></returns>
    protected bool AddShader(ShaderType shaderType, string shaderSource)
    {
      int shader = GL.CreateShader(shaderType);
      GL.ShaderSource(shader, shaderSource);
      GL.CompileShader(shader);
      int status;
      GL.GetShader(shader, ShaderParameter.CompileStatus, out status);
      if(status == 1)
      {
        GL.AttachShader(_shaderProgramID, shader);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Links the shader program.
    /// </summary>
    protected void LinkShaderProgram()
    {
      GL.LinkProgram(_shaderProgramID);
    }

    /// <summary>
    /// Changes the attribute.
    /// </summary>
    /// <param name="bufferTarget">The VBO type.</param>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <param name="size">The size.</param>
    /// <param name="type">The type.</param>
    /// <param name="normalized">if set to <c>true</c> [normalized].</param>
    /// <param name="stride">The stride.</param>
    /// <param name="offset">The offset.</param>
    protected void ChangeAttribute(BufferTarget bufferTarget, string attributeName, int size, VertexAttribPointerType type,
                                bool normalized, int stride, int offset)
    {
      Vao.BindWithVBO(bufferTarget);

      if(!_attribLocations.ContainsKey(attributeName))
      {
        _attribLocations.Add(attributeName, GL.GetAttribLocation(_shaderProgramID, attributeName));
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
      Vao.UnBindWithVBO(bufferTarget);
    }

    /// <summary>
    /// Sets this shader program as active on GPU
    /// </summary>
    private void StartUseProgram()
    {
      GL.UseProgram(_shaderProgramID);
    }

    /// <summary>
    /// Sets this shader program as inactive on GPU
    /// </summary>
    private static void StopUseProgram()
    {
      GL.UseProgram(0);
    }

    /// <summary>
    /// Uploads the data to GPU.
    /// </summary>
    public abstract void UploadDataToGPU();

    /// <summary>
    /// Draws next primitive
    /// </summary>
    public abstract void DrawNext();

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
      CurrentTask = 0;
      Verticies.Clear();
    }

    /// <summary>
    /// Upload 4x4 matrix to gpu
    /// </summary>
    /// <param name="matrixName">Name of the matrix in shader</param>
    /// <param name="matrix">The matrix.</param>
    /// <param name="transpose">if set to <c>true</c> [transpose].</param>
    public void UniformMatrix4(string matrixName, Matrix4 matrix, bool transpose)
    {
      StartUseProgram();
      GL.UniformMatrix4(GetUniformLocation(matrixName), transpose, ref matrix);
      StopUseProgram();
    }

    /// <summary>
    /// Gets the uniform location from gpu or from cache
    /// </summary>
    /// <param name="parametrName">Name of the parametr.</param>
    /// <returns></returns>
    private int GetUniformLocation(string parametrName)
    {
      if(!_uniformLocations.ContainsKey(parametrName))
      {
        _uniformLocations.Add(parametrName, GL.GetUniformLocation(_shaderProgramID, parametrName));
      }
      if(_uniformLocations[parametrName] == -1)
      {
        throw new ShaderParametrNotFoundException();
      }
      return _uniformLocations[parametrName];
    }

    #region Uniform1

    public void Uniform1(string parametrName, float value)
    {
      StartUseProgram();
      GL.Uniform1(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    public void Uniform1(string parametrName, double value)
    {
      StartUseProgram();
      GL.Uniform1(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    public void Uniform1(string parametrName, int value)
    {
      StartUseProgram();
      GL.Uniform1(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    public void Uniform1(string parametrName, uint value)
    {
      StartUseProgram();
      GL.Uniform1(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    #endregion

    #region Uniform2

    protected void Uniform2(string parametrName, Vector2 value)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    protected void Uniform2(string parametrName, ref Vector2 value)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), ref value);
      StopUseProgram();
    }

    protected void Uniform2(string parametrName, double x, double y)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), x, y);
      StopUseProgram();
    }

    protected void Uniform2(string parametrName, float x, float y)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), x, y);
      StopUseProgram();
    }

    protected void Uniform2(string parametrName, uint v0, uint v1)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), v0, v1);
      StopUseProgram();
    }

    protected void Uniform2(string parametrName, int v0, int v1)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), v0, v1);
      StopUseProgram();
    }

    protected void Uniform2(string parametrName, int count, float[] value)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    protected void Uniform2(string parametrName, int count, double[] value)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    protected void Uniform2(string parametrName, int count, int[] value)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    protected void Uniform2(string parametrName, int count, uint[] value)
    {
      StartUseProgram();
      GL.Uniform2(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    #endregion

    #region Uniform3

    protected void Uniform3(string parametrName, Vector3 value)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    protected void Uniform3(string parametrName, ref Vector3 value)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), ref value);
      StopUseProgram();
    }

    protected void Uniform3(string parametrName, double x, double y, double z)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), x, y, z);
      StopUseProgram();
    }

    protected void Uniform3(string parametrName, float x, float y, float z)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), x, y, z);
      StopUseProgram();
    }

    protected void Uniform3(string parametrName, uint v0, uint v1, uint v2)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), v0, v1, v2);
      StopUseProgram();
    }

    protected void Uniform3(string parametrName, int v0, int v1, int v2)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), v0, v1, v2);
      StopUseProgram();
    }

    protected void Uniform3(string parametrName, int count, float[] value)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    protected void Uniform3(string parametrName, int count, double[] value)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    protected void Uniform3(string parametrName, int count, int[] value)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    protected void Uniform3(string parametrName, int count, uint[] value)
    {
      StartUseProgram();
      GL.Uniform3(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    #endregion

    #region Uniform4

    protected void Uniform4(string parametrName, Vector4 value)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), value);
      StopUseProgram();
    }

    protected void Uniform4(string parametrName, ref Vector4 value)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), ref value);
      StopUseProgram();
    }

    protected void Uniform4(string parametrName, double x, double y, double z, double w)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), x, y, z, w);
      StopUseProgram();
    }

    protected void Uniform4(string parametrName, float x, float y, float z, float w)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), x, y, z, w);
      StopUseProgram();
    }

    protected void Uniform4(string parametrName, uint v0, uint v1, uint v2, uint v3)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), v0, v1, v2, v3);
      StopUseProgram();
    }

    protected void Uniform4(string parametrName, int v0, int v1, int v2, int v3)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), v0, v1, v2, v3);
      StopUseProgram();
    }

    protected void Uniform4(string parametrName, int count, float[] value)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    protected void Uniform4(string parametrName, int count, double[] value)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    protected void Uniform4(string parametrName, int count, int[] value)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    protected void Uniform4(string parametrName, int count, uint[] value)
    {
      StartUseProgram();
      GL.Uniform4(GetUniformLocation(parametrName), count, value);
      StopUseProgram();
    }

    #endregion

    /// <summary>
    /// Draws the arrays.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="first">The first.</param>
    /// <param name="count">The count.</param>
    protected void DrawArrays(BeginMode mode, int first, int count)
    {
      StartUseProgram();
      Vao.Bind();
      GL.DrawArrays(mode, first, count);
      Vao.UnBind();
      StopUseProgram();
    }

    /// <summary>
    /// Draws the elements.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="count">The count.</param>
    /// <param name="type">The type.</param>
    protected void DrawElements(BeginMode mode, int count, DrawElementsType type = DrawElementsType.UnsignedInt)
    {
      if(!Vao.VBOexists(BufferTarget.ElementArrayBuffer))
      {
        throw new VBONotFoundException("Index VBO not found");
      }
      StartUseProgram();
      Vao.BindWithVBO(BufferTarget.ElementArrayBuffer);
      GL.DrawElements(mode, count, type, IntPtr.Zero);
      Vao.UnBindWithVBO(BufferTarget.ElementArrayBuffer);
      StopUseProgram();
    }

    #region Implementation of IDisposable

    /// <summary>
    /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
      Vao.Dispose();
      GL.DeleteProgram(_shaderProgramID);
    }

    #endregion
  }
}