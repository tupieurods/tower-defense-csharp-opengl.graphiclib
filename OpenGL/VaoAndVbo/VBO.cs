using System;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL
{
  /// <summary>
  /// Don't create this class manually, from VAO class only
  /// </summary>
  internal class VBO: IDisposable
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
    /// VBO buffer usage hint, correct usage hint increases performance
    /// </summary>
    private readonly BufferUsageHint _bufferUsageHint;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="VBO"/> class.
    /// </summary>
    /// <param name="bufferTarget">Logical type of data in VBO</param>
    /// <param name="bufferUsageHint">The buffer usage hint.</param>
    /// <param name="size">The size.</param>
    internal VBO(BufferTarget bufferTarget, BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw, int size = 0)
    {
      GL.GenBuffers(1, out _vbo);
      _bufferTarget = bufferTarget;
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
      if(_currentSize >= size)
      {
        return;
      }
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
      if(size > _currentSize)
      {
        Resize(size);
      }
      GL.Ext.NamedBufferSubData(_vbo, new IntPtr(offset), new IntPtr(size), dataBuffer);
      var errorCode = GL.GetError();
      return errorCode == ErrorCode.NoError;
    }

    /// <summary>
    /// Changes the data in VBO.
    /// </summary>
    /// <param name="dataBuffer">The data buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>true if successful,false if error</returns>
    internal bool ChangeDataInVBO(float[,] dataBuffer, int offset = 0)
    {
      int size = dataBuffer.Length * sizeof(float);
      if(size > _currentSize)
      {
        Resize(size);
      }
      GL.Ext.NamedBufferSubData(_vbo, new IntPtr(offset), new IntPtr(size), dataBuffer);
      var errorCode = GL.GetError();
      return errorCode == ErrorCode.NoError;
    }

    //ForDebugOnly
    internal bool ChangeDataInVBO(double[] dataBuffer, int offset = 0)
    {
      int size = dataBuffer.Length * sizeof(double);
      if(size > _currentSize)
      {
        Resize(size);
      }
      GL.Ext.NamedBufferSubData(_vbo, new IntPtr(offset), new IntPtr(size), dataBuffer);
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
      if(size > _currentSize)
      {
        Resize(size);
      }
      GL.Ext.NamedBufferSubData(_vbo, new IntPtr(offset), new IntPtr(size), dataBuffer);
      var errorCode = GL.GetError();
      return errorCode == ErrorCode.NoError;
    }

    /// <summary>
    /// Changes the data in VBO.
    /// </summary>
    /// <param name="dataBuffer">The data buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>true if successful,false if error</returns>
    internal bool ChangeDataInVBO(int[] dataBuffer, int offset = 0)
    {
      int size = dataBuffer.Length * sizeof(uint);
      if(size > _currentSize)
      {
        Resize(size);
      }
      GL.Ext.NamedBufferSubData(_vbo, new IntPtr(offset), new IntPtr(size), dataBuffer);
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