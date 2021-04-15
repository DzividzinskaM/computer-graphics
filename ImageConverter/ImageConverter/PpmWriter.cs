using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageConverter
{
    public class PpmWriter : IImageWriter
    {
        private const string PPM_DEFINED_FORMAT = "P6";
        private const string MAX_COLOR = "255";
        private const byte LINE_FEED_ASCII_CHARACTER = 10;
        private const byte SPACE_ASCII_CHARACTER = 32;

        private ASCIIEncoding ascii = new ASCIIEncoding();

        public void Write(string filePath, Image image)
        {
            FileStream fileStream = File.Create(filePath);
            WriteHeader(fileStream, image);
            WriteData(fileStream, image);
            fileStream.Close();
        }

        private void WriteData(FileStream ppmFile, Image image)
        {
            ppmFile.Write(image.RGBData);
        }

        private void WriteHeader(FileStream ppmFile, Image image)
        {
            ppmFile.Write(ascii.GetBytes(PPM_DEFINED_FORMAT));
            WriteNextLine(ppmFile);
            ppmFile.Write(ascii.GetBytes(image.Width.ToString()));
            WriteSpace(ppmFile);
            ppmFile.Write(ascii.GetBytes(image.Length.ToString()));
            WriteNextLine(ppmFile);
            ppmFile.Write(ascii.GetBytes(MAX_COLOR));
            WriteNextLine(ppmFile);

        }

        private void WriteSpace(FileStream ppmFile)
        {
            ppmFile.WriteByte(SPACE_ASCII_CHARACTER);
        }

        private void WriteNextLine(FileStream ppmFile)
        {
            ppmFile.WriteByte(LINE_FEED_ASCII_CHARACTER);
        }
    }
}
