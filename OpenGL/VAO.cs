using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace GraphicLib.OpenGL
{
  public class VAO: IDisposable
  {
    /// <summary>
    /// VAO index on GPU
    /// </summary>
    private int _vao;

    /// <summary>
    /// VBOs, which associated with this VAO
    /// </summary>
    private readonly Dictionary<BufferTarget, VBO> VBOobjects = new Dictionary<BufferTarget, VBO>();

    /// <summary>
    /// Initializes a new instance of the <see cref="VAO"/> class.
    /// </summary>
    /// <param name="bufferTargets">The VBO data types.</param>
    /// <param name="VBObufferUsageHint">The VBO buffer usage hint.</param>
    /// <param name="VBOsize">The VBO size.</param>
    public VAO(BufferTarget[] bufferTargets = null, BufferUsageHint VBObufferUsageHint = BufferUsageHint.DynamicDraw,
               int VBOsize = 0)
    {
      GL.GenVertexArrays(1, out _vao);
      GL.BindVertexArray(_vao);
      if(bufferTargets != null)
      {
        foreach(var buffer in bufferTargets.Where(x => !VBOobjects.ContainsKey(x)))
        {
          VBOobjects.Add(buffer, new VBO(buffer, VBObufferUsageHint, VBOsize));
        }
      }
      GL.BindVertexArray(0);
    }

    internal void Resize(BufferTarget bufferTarget, int size)
    {
      if(!VBOexists(bufferTarget))
      {
        VBOobjects.Add(bufferTarget, new VBO(bufferTarget));
      }
      Bind();
      VBOobjects[bufferTarget].Resize(size);
      UnBind();
    }

    #region VAO data changers

    /// <summary>
    /// Changes stored data
    /// </summary>
    /// <param name="bufferTarget">The VBO type in VAO.</param>
    /// <param name="dataBuffer">The vertex buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>True if data changes successful</returns>
    internal bool ChangeData(BufferTarget bufferTarget, float[] dataBuffer, int offset = 0)
    {
      if(!VBOexists(bufferTarget))
      {
        VBOobjects.Add(bufferTarget, new VBO(bufferTarget));
      }
      Bind();
      bool result = VBOobjects[bufferTarget].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    /// <summary>
    /// Changes stored data
    /// </summary>
    /// <param name="bufferTarget">The VBO type in VAO.</param>
    /// <param name="dataBuffer">The vertex buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>True if data changes successful</returns>
    internal bool ChangeData(BufferTarget bufferTarget, float[,] dataBuffer, int offset = 0)
    {
      if(!VBOexists(bufferTarget))
      {
        VBOobjects.Add(bufferTarget, new VBO(bufferTarget));
      }
      Bind();
      bool result = VBOobjects[bufferTarget].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    //ForDebugOnly
    internal bool ChangeData(BufferTarget bufferTarget, double[] dataBuffer, int offset = 0)
    {
      if(!VBOexists(bufferTarget))
      {
        VBOobjects.Add(bufferTarget, new VBO(bufferTarget));
      }
      Bind();
      bool result = VBOobjects[bufferTarget].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    /// <summary>
    /// Changes stored data
    /// </summary>
    /// <param name="bufferTarget">The VBO type in VAO.</param>
    /// <param name="dataBuffer">The vertex buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>True if data changes successful</returns>
    internal bool ChangeData(BufferTarget bufferTarget, uint[] dataBuffer, int offset = 0)
    {
      if(!VBOexists(bufferTarget))
      {
        VBOobjects.Add(bufferTarget, new VBO(bufferTarget));
      }
      Bind();
      bool result = VBOobjects[bufferTarget].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    /// <summary>
    /// Changes stored data
    /// </summary>
    /// <param name="bufferTarget">The VBO type in VAO.</param>
    /// <param name="dataBuffer">The vertex buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>True if data changes successful</returns>
    internal bool ChangeData(BufferTarget bufferTarget, int[] dataBuffer, int offset = 0)
    {
      if(!VBOexists(bufferTarget))
      {
        VBOobjects.Add(bufferTarget, new VBO(bufferTarget));
      }
      Bind();
      bool result = VBOobjects[bufferTarget].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    #endregion

    /// <summary>
    /// Checks, have we VBO with specified data or not
    /// </summary>
    /// <param name="bufferTarget">The VBO type.</param>
    /// <returns>True, if we have VBO with specified data</returns>
    internal bool VBOexists(BufferTarget bufferTarget)
    {
      return VBOobjects.ContainsKey(bufferTarget);
    }

    #region VAO binders

    /// <summary>
    /// Binds this instance.
    /// </summary>
    internal void Bind()
    {
      GL.BindVertexArray(_vao);
    }

    /// <summary>
    /// Unbinds this instance
    /// </summary>
    internal void UnBind()
    {
      GL.BindVertexArray(0);
    }

    /// <summary>
    /// Binds VAO with specified VBO.
    /// </summary>
    /// <param name="bufferTarget">The VBO type.</param>
    internal void BindWithVBO(BufferTarget bufferTarget)
    {
      if(!VBOobjects.ContainsKey(bufferTarget))
      {
        throw new ArgumentException("bufferTarget");
      }
      GL.BindVertexArray(_vao);
      VBOobjects[bufferTarget].Bind();
    }

    /// <summary>
    /// Unbinds VAO with specified VBO.
    /// </summary>
    /// <param name="bufferTarget">The VBO type.</param>
    internal void UnBindWithVBO(BufferTarget bufferTarget)
    {
      if(!VBOobjects.ContainsKey(bufferTarget))
      {
        throw new ArgumentException("bufferTarget");
      }
      GL.BindVertexArray(0);
      VBOobjects[bufferTarget].UnBind();
    }

    #endregion

    #region Implementation of IDisposable

    /// <summary>
    /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
      foreach(var vboObject in VBOobjects)
      {
        vboObject.Value.Dispose();
      }
      GL.DeleteVertexArrays(1, ref _vao);
    }

    #endregion
  }
}