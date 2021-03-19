using System;
using System.Collections.Generic;
using System.Text;

namespace ImageConverter
{
    interface IImageReader
    {
        public void Read(string filePath);
    }
}
