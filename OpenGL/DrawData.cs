using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GraphicLib.OpenGL
{
  //Currently implemented only SolidBrush
  //No needs in other
  internal struct EllipseData
  {
    internal float Xc;
    internal float Yc;
    internal float Xr;
    internal float Yr;
    internal Color color;
    internal float border;
  }
}
