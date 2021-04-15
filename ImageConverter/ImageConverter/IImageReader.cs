using System;
using System.Collections.Generic;
using System.Text;

namespace ImageConverter
{
    public interface IImageReader
    {
        public Image Read(string filePath);
    }
}
