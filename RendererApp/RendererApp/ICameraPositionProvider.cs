using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public interface ICameraPositionProvider
    {
        public Vector3 GetCameraPosition();
    }
}
