using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class StaticCameraPositionProvider : ICameraPositionProvider
    {
        public Vector3 GetCameraPosition()
        {
            return new Vector3(0, 0, 1);
        }
    }
}
