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
    private readonly Dictionary<VBOdata, VBO> VBOobjects = new Dictionary<VBOdata, VBO>();

    /// <summary>
    /// Initializes a new instance of the <see cref="VAO"/> class.
    /// </summary>
    /// <param name="VBOtypes">The VBO data types.</param>
    /// <param name="VBObufferUsageHint">The VBO buffer usage hint.</param>
    /// <param name="VBOsize">The VBO size.</param>
    public VAO(VBOdata[] VBOtypes = null, BufferUsageHint VBObufferUsageHint = BufferUsageHint.DynamicDraw,
               int VBOsize = 0)
    {
      GL.GenVertexArrays(1, out _vao);
      GL.BindVertexArray(_vao);
      if(VBOtypes != null)
      {
        foreach(var vbotype in VBOtypes.Where(vbotype => !VBOobjects.ContainsKey(vbotype)))
        {
          VBOobjects.Add(vbotype, new VBO(vbotype, VBObufferUsageHint, VBOsize));
        }
      }
      GL.BindVertexArray(0);
    }

    internal void Resize(VBOdata VBOtype, int size)
    {
      if(!VBOexists(VBOtype))
      {
        VBOobjects.Add(VBOtype, new VBO(VBOtype));
      }
      Bind();
      VBOobjects[VBOtype].Resize(size);
      UnBind();
    }

    #region VAO data changers

    /// <summary>
    /// Changes stored data
    /// </summary>
    /// <param name="VBOtype">The VBO type in VAO.</param>
    /// <param name="dataBuffer">The vertex buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>True if data changes successful</returns>
    internal bool ChangeData(VBOdata VBOtype, float[] dataBuffer, int offset = 0)
    {
      if(!VBOexists(VBOtype))
      {
        VBOobjects.Add(VBOtype, new VBO(VBOtype));
      }
      Bind();
      bool result = VBOobjects[VBOtype].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    /// <summary>
    /// Changes stored data
    /// </summary>
    /// <param name="VBOtype">The VBO type in VAO.</param>
    /// <param name="dataBuffer">The vertex buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>True if data changes successful</returns>
    internal bool ChangeData(VBOdata VBOtype, float[,] dataBuffer, int offset = 0)
    {
      if(!VBOexists(VBOtype))
      {
        VBOobjects.Add(VBOtype, new VBO(VBOtype));
      }
      Bind();
      bool result = VBOobjects[VBOtype].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    //ForDebugOnly
    internal bool ChangeData(VBOdata VBOtype, double[] dataBuffer, int offset = 0)
    {
      if(!VBOexists(VBOtype))
      {
        VBOobjects.Add(VBOtype, new VBO(VBOtype));
      }
      Bind();
      bool result = VBOobjects[VBOtype].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    /// <summary>
    /// Changes stored data
    /// </summary>
    /// <param name="VBOtype">The VBO type in VAO.</param>
    /// <param name="dataBuffer">The vertex buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>True if data changes successful</returns>
    internal bool ChangeData(VBOdata VBOtype, uint[] dataBuffer, int offset = 0)
    {
      if(!VBOexists(VBOtype))
      {
        VBOobjects.Add(VBOtype, new VBO(VBOtype));
      }
      Bind();
      bool result = VBOobjects[VBOtype].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    /// <summary>
    /// Changes stored data
    /// </summary>
    /// <param name="VBOtype">The VBO type in VAO.</param>
    /// <param name="dataBuffer">The vertex buffer.</param>
    /// <param name="offset">Buffer offset </param>
    /// <returns>True if data changes successful</returns>
    internal bool ChangeData(VBOdata VBOtype, int[] dataBuffer, int offset = 0)
    {
      if(!VBOexists(VBOtype))
      {
        VBOobjects.Add(VBOtype, new VBO(VBOtype));
      }
      Bind();
      bool result = VBOobjects[VBOtype].ChangeDataInVBO(dataBuffer, offset);
      UnBind();
      return result;
    }

    #endregion

    /// <summary>
    /// Checks, have we VBO with specified data or not
    /// </summary>
    /// <param name="VBOtype">The VBO type.</param>
    /// <returns>True, if we have VBO with specified data</returns>
    internal bool VBOexists(VBOdata VBOtype)
    {
      return VBOobjects.ContainsKey(VBOtype);
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
    /// <param name="VBOtype">The VBO type.</param>
    internal void BindWithVBO(VBOdata VBOtype)
    {
      if(!VBOobjects.ContainsKey(VBOtype))
      {
        throw new ArgumentException("VBOtype");
      }
      GL.BindVertexArray(_vao);
      VBOobjects[VBOtype].Bind();
    }

    /// <summary>
    /// Unbinds VAO with specified VBO.
    /// </summary>
    /// <param name="VBOtype">The VBO type.</param>
    internal void UnBindWithVBO(VBOdata VBOtype)
    {
      if(!VBOobjects.ContainsKey(VBOtype))
      {
        throw new ArgumentException("VBOtype");
      }
      GL.BindVertexArray(0);
      VBOobjects[VBOtype].UnBind();
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