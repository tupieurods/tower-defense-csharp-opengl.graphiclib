using System;

namespace GraphicLib.OpenGL
{
  class ShaderParametrNotFoundException : Exception
  {
    public ShaderParametrNotFoundException()
    {
    }
    public ShaderParametrNotFoundException(string str)
      : base(str)
    {
    }
    public ShaderParametrNotFoundException(string str, Exception inner)
      : base(str, inner)
    {
    }
    public ShaderParametrNotFoundException(
      System.Runtime.Serialization.SerializationInfo si,
      System.Runtime.Serialization.StreamingContext sc)
      : base(si, sc)
    {
    }

    public override string ToString()
    {
      return Message;
    }
  }
}