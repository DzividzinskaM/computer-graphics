using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public interface IProgramSceneObject
    {
        public Transform transform { get; set; }
        public IMaterial material { get; set; }
    }
}
