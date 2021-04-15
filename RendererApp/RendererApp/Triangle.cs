using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class Triangle
    {
        public Vector3 a { get; }
        public Vector3 b { get; }
        public Vector3 c { get; }

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }
}
