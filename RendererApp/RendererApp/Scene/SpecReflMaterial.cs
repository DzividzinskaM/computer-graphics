using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class SpecReflMaterial : IMaterial
    {
        public float eta { get; set; }
        public SpecReflMaterial(float eta)
        {
            this.eta = eta;
        }
    }
}
