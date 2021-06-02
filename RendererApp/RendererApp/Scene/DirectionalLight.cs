using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class DirectionalLight : ILightSource
    {
        public float Intencity { get; set; }
        public Vector3 Direction { get; set; }
    }
}
