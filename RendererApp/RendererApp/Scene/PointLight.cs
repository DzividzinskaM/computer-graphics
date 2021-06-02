using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class PointLight : ILightSource
    {
        public float Intencity { get; set; }
        public Vector3 Position { get; set; }
    }
}
