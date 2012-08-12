using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGl
{
  internal class ShaderProgram : IDisposable
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
    /// Initializes a new instance of the <see cref="ShaderProgram"/> class.
    /// </summary>
    /// <param name="vertexShaderSource">The vertex shader source.</param>
    /// <param name="fragmentShaderSource">The fragment shader source.</param>
    /// <param name="vao">The vao.</param>
    internal ShaderProgram(byte[] vertexShaderSource, byte[] fragmentShaderSource, VAO vao = null)
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
      GL.CompileShader(fragmentShader);
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
    internal void AttachVAO(VAO vao)
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
    internal void ChangeAttribute(VBOdata VBOtype, string attributeName, int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
    {
      UsePorgram();
      _vao.BindWithVBO(VBOtype);
      int attributeLocation = GL.GetAttribLocation(_shaderProgram, attributeName);
      if (attributeLocation != -1)
      {
        GL.VertexAttribPointer(attributeLocation, size, type, normalized, stride, offset);
        GL.EnableVertexAttribArray(attributeLocation);
      }
      _vao.UnBindWithVBO(VBOtype);
      StopUseProgram();
    }

    /// <summary>
    /// Changes the data in VAO
    /// </summary>
    /// <param name="VBOtype">The VBO type.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns></returns>
    internal bool ChangeData(VBOdata VBOtype, float[] buffer)
    {
      UsePorgram();
      bool result = _vao.ChangeData(VBOtype, buffer);
      StopUseProgram();
      return result;
    }

    /// <summary>
    /// Changes the data in VAO
    /// </summary>
    /// <param name="VBOtype">The VBO type.</param>
    /// <param name="buffer">The buffer.</param>
    /// <returns></returns>
    internal bool ChangeData(VBOdata VBOtype, uint[] buffer)
    {
      UsePorgram();
      bool result = _vao.ChangeData(VBOtype, buffer);
      StopUseProgram();
      return result;
    }

    /// <summary>
    /// Sets this shader program as active on GPU
    /// </summary>
    internal void UsePorgram()
    {
      GL.UseProgram(_shaderProgram);
    }

    /// <summary>
    /// Sets this shader program as inactive on GPU
    /// </summary>
    internal void StopUseProgram()
    {
      GL.UseProgram(0);
    }

    /// <summary>
    /// Uniforms the matrix4.
    /// </summary>
    /// <param name="matrixName">Name of the matrix.</param>
    /// <param name="matrix">The matrix.</param>
    /// <param name="transpose">if set to <c>true</c> [transpose].</param>
    internal void UniformMatrix4(string matrixName, float[] matrix, bool transpose)
    {
      UsePorgram();
      int projectionMatrixLocation = GL.GetUniformLocation(_shaderProgram, matrixName);
      if (projectionMatrixLocation != -1)
        GL.UniformMatrix4(projectionMatrixLocation, 1, transpose, matrix);
      StopUseProgram();
    }

    /// <summary>
    /// Draws the arrays.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="first">The first.</param>
    /// <param name="count">The count.</param>
    internal void DrawArrays(BeginMode mode, int first, int count)
    {
      UsePorgram();
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
    internal void DrawElements(BeginMode mode, int count, DrawElementsType type = DrawElementsType.UnsignedInt)
    {
      if (!_vao.VBOexists(VBOdata.Index))
        throw new VBONotFoundException("Index VBO not found");
      UsePorgram();
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