using System;

namespace GraphicLib.OpenGl
{
  class VBONotFoundException : Exception
  {
    public VBONotFoundException()
    {
    }
    public VBONotFoundException(string str)
      : base(str)
    {
    }
    public VBONotFoundException(string str, Exception inner)
      : base(str, inner)
    {
    }
    public VBONotFoundException(
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