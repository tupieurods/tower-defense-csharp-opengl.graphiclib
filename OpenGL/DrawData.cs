using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GraphicLib.OpenGL
{
  internal struct FillEllipseData
  {
    internal SolidBrush EllipseBrush;
    internal float X;
    internal float Y;
    internal float Width;
    internal float Height;
  }

  internal struct DrawEllipseData
  {
    internal Pen EllipsePen;
    internal float X;
    internal float Y;
    internal float Width;
    internal float Height;
  }
}
