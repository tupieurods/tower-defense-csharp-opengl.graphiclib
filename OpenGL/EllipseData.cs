using System.Drawing;

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
    internal Color Color;
    internal float Border;
  }
}