﻿using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGl
{
  /// <summary>
  /// Don't create this class manually, from VAO class only
  /// </summary>
  internal class VBO : IDisposable
  {
    #region Private data
    /// <summary>
    /// VBO handle
    /// </summary>
    private int _vbo;

    /// <summary>
    /// Current VBO size. May only grow.
    /// </summary>
    private int _currentSize;

    /// <summary>
    /// VBO buffer target on GPU
    /// </summary>
    private readonly BufferTarget _bufferTarget;

    /// <summary>
    /// VBO buffer usage hint, correct usage hint increases perfomance
    /// </summary>
    private readonly BufferUsageHint _bufferUsageHint;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="VBO"/> class.
    /// </summary>
    /// <param name="VBOtype">Logical type of data in vbo</param>
    /// <param name="bufferUsageHint">The buffer usage hint.</param>
    /// <param name="size">The size.</param>
    internal VBO(VBOdata VBOtype, BufferUsageHint bufferUsageHint = BufferUsageHint.StreamDraw, int size = 0)
    {
      GL.GenBuffers(1, out _vbo);
      _bufferTarget = GetBufferTargetByType(VBOtype);
      GL.BindBuffer(_bufferTarget, _vbo);
      GL.BufferData(_bufferTarget, new IntPtr(size), new IntPtr(0), bufferUsageHint);
      GL.BindBuffer(_bufferTarget, 0);
      _currentSize = size;
      _bufferUsageHint = bufferUsageHint;
    }

    /// <summary>
    /// Resizes vbo.
    /// </summary>
    /// <param name="size">The new size.</param>
    internal void Resize(int size)
    {
      if (_currentSize >= size)
        return;
      GL.DeleteBuffers(1, ref _vbo);
      GL.GenBuffers(1, out _vbo);
      Bind();
      GL.BufferData(_bufferTarget, new IntPtr(size), new IntPtr(0), _bufferUsageHint);
      UnBind();
      _currentSize = size;
    }

    #region VBO Data changers

    /// <summary>
    /// Changes the data in VBO.
    /// </summary>
    /// <param name="dataBuffer">The data buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>true if successful,false if error</returns>
    internal bool ChangeDataInVBO(float[] dataBuffer, int offset = 0)
    {
      int size = dataBuffer.Length * sizeof(float);
      if (size > _currentSize)
        Resize(size);
      Bind();
      GL.BufferSubData(_bufferTarget, new IntPtr(offset), new IntPtr(size), dataBuffer);
      UnBind();
      var errorCode = GL.GetError();
      return errorCode == ErrorCode.NoError;
    }

    /// <summary>
    /// Changes the data in VBO.
    /// </summary>
    /// <param name="dataBuffer">The data buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>true if successful,false if error</returns>
    internal bool ChangeDataInVBO(uint[] dataBuffer, int offset = 0)
    {
      int size = dataBuffer.Length * sizeof(uint);
      if (size > _currentSize)
        Resize(size * 2);
      Bind();
      GL.BufferSubData(_bufferTarget, new IntPtr(offset), new IntPtr(dataBuffer.Length * sizeof(uint)), dataBuffer);
      UnBind();
      var errorCode = GL.GetError();
      return errorCode == ErrorCode.NoError;
    }
    #endregion

    #region VBO binders
    /// <summary>
    /// Binds this instance.
    /// </summary>
    internal void Bind()
    {
      GL.BindBuffer(_bufferTarget, _vbo);
    }

    /// <summary>
    /// Unbinds this instance
    /// </summary>
    internal void UnBind()
    {
      GL.BindBuffer(_bufferTarget, 0);
    }
    #endregion

    /// <summary>
    /// Get the type of buffer for the data type
    /// </summary>
    /// <param name="VBOtype">The VBO data type.</param>
    /// <returns></returns>
    private static BufferTarget GetBufferTargetByType(VBOdata VBOtype)
    {
      BufferTarget result;
      switch (VBOtype)
      {
        case VBOdata.Positions:
        case VBOdata.Color:
        case VBOdata.TextureCoord:
          result = BufferTarget.ArrayBuffer;
          break;
        case VBOdata.Index:
          result = BufferTarget.ElementArrayBuffer;
          break;
        default:
          throw new ArgumentOutOfRangeException("VBOtype");
      }
      return result;
    }

    #region Implementation of IDisposable

    /// <summary>
    /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
      GL.DeleteBuffers(1, ref _vbo);
    }

    #endregion
  }
}
