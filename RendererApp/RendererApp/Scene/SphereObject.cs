using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class SphereObject : IProgramSceneObject
    {
        public Transform transform { get; set; }
        public IMaterial material { get; set; }
        public float radius { get; set; }
    }
}
