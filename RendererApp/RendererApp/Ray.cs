using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class Ray
    {
        public Vector3 Orig { get; set; }
        public Vector3 Dir { get; set; }

        public Ray(Vector3 origin, Vector3 dir)
        {
            Orig = origin;
            Dir = dir;
        }

        public Vector3 At(float t)
        {
            return (Orig + t * Dir);
        }
    }
}
