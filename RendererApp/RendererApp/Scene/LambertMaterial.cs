using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class LambertMaterial : IMaterial
    {
        public Vector3 color { get; set; }

        public LambertMaterial(Vector3 color)
        {
            this.color = color;
        }
    }
}
