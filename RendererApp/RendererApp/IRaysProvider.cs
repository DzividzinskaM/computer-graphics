using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public interface IRaysProvider
    {
        public List<Ray> GetRays(int imageWidth, int imageHeight, Vector3 cameraPos, Screen screen);
        
    }
}
