using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicLib.OpenGL
{
  static class Matrix
  {
    /// <summary>
    /// Sets orthogonal projection matrix
    /// </summary>
    /// <param name="projectionMatrix">The projection matrix.</param>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <param name="bottom">The bottom.</param>
    /// <param name="top">The top.</param>
    /// <param name="zNear">The z near.</param>
    /// <param name="zFar">The z far.</param>
    static internal void Matrix4Ortho(IList<float> projectionMatrix, float left, float right, float bottom, float top, float zNear, float zFar)
    {
      float tx = -(right + left) / (right - left),
            ty = -(top + bottom) / (top - bottom),
            tz = -(zFar + zNear) / (zFar - zNear);
      projectionMatrix[0] = 2 / (right - left); projectionMatrix[1] = 0; projectionMatrix[2] = 0; projectionMatrix[3] = tx;
      projectionMatrix[4] = 0; projectionMatrix[5] = 2 / (top - bottom); projectionMatrix[6] = 0; projectionMatrix[7] = ty;
      projectionMatrix[8] = 0; projectionMatrix[9] = 0; projectionMatrix[10] = -2 / (zFar - zNear); projectionMatrix[11] = tz;
      projectionMatrix[12] = 0; projectionMatrix[13] = 0; projectionMatrix[14] = 0; projectionMatrix[15] = 1;
    }
  }
}
