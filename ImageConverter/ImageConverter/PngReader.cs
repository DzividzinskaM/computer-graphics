using System;
using System.Collections.Generic;
using System.Text;

namespace ImageConverter
{
    class PngReader : IImageReader
    {

        public void Read(string filePath)
        {
            Console.WriteLine("Reading...");
        }
    }
}
