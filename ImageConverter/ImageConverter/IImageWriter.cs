using System;
using System.Collections.Generic;
using System.Text;

namespace ImageConverter
{
    interface IImageWriter
    {
        public void Write(string filePath, Image image);
    }
}
