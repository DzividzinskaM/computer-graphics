using System;
using System.Collections.Generic;
using System.Text;

namespace ImageConverter
{
    public class Pixel
    {
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }

        public byte alpha = 255;

        public Pixel(byte r, byte g, byte b)
        {
            this.r = r;
            this.b = b;
            this.g = g;
        }
    }
}
