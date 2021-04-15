using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class RaysPerspectiveProvider : IRaysProvider
    {
        public List<Ray> GetRays(int imageWidth, int imageHeight, Vector3 cameraPos, Screen screen)
        {
            List<Ray> rays = new List<Ray>();
            for (int j = imageHeight - 1; j >= 0; --j)
            {
                for (int i = 0; i < imageWidth; ++i)
                {
                    var u = (float)i / (imageWidth - 1);
                    var v = (float)j / (imageHeight - 1);
                    rays.Add(new Ray(cameraPos, screen.LowerLeftCorner +
                        u * screen.Horizontal + v * screen.Vertical - cameraPos));
                }
            }
            return rays;
        }
    }
}
