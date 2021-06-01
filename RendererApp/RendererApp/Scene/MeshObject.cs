using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class MeshObject : IProgramSceneObject
    {
        public string filePath { get; set; }
        public Transform transform { get; set; }
        public IMaterial material { get; set; }

    }
}
