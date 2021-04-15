using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class Screen
    {
        public Vector3 Horizontal { get; set; }
        public Vector3 Vertical { get; set; }
        public Vector3 LowerLeftCorner { get; set; }

        public Screen(float height, float width, float focal_length, Vector3 cameraPos)
        { 
            Horizontal = new Vector3(width, 0, 0);
            Vertical = new Vector3(0, height, 0);
            LowerLeftCorner = cameraPos - 0.5f * Horizontal - 0.5f * Vertical - new Vector3(0, 0, focal_length);
        }
    }
}
