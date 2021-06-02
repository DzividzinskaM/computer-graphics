using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class StaticCameraPositionProvider : ICameraPositionProvider
    {
        public Vector3 position { get; set; }
        public int id { get; set; }

        public void SetCameraPosition(Vector3 position, int id)
        {
            this.position = position;
            this.id = id;
        }

        public Vector3 GetCameraPosition()
        {
            return position;
        }
    }
}
