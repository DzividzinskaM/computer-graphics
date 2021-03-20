using System;
using System.Collections.Generic;
using System.Text;

namespace ImageConverter
{
    class Image
    {
        public int Width { get; }

        public int Length { get; }

        public byte[] RGBData { get; }

        public Image(int width, int length, byte[] rgbData)
        {
            Width = width;
            Length = length;
            RGBData = rgbData;
        }
    }
}
