using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageConverter
{
    class PngReader : IImageReader
    {
        public byte[] inputBytes;

        private const string PNG_DEFINED_BYTES = "89504E47DA1AA";
        private const int START_INDEX = 0;
        private const int NUMBER_DEFINITION_FORMAT_BYTES = 8;
        private const string IHDR = "IHDR";
        private const string PLTE = "PLTE";
        private const string IDAT = "IDAT";
        private const string IEND = "IEND";
        private const int ONE_BYTE_BLOCK = 1;
        private const int FOUR_BYTES_BLOCK = 4;
        private const int WIDTH_START_POSITION = 0;
        private const int LENGTH_START_POSITION = 4;
        private const int BIT_DEPTH_START_POSITION = 8;
        private const int COLOR_TYPE_START_POSITION = 9;
        private const int COMPRESSION_TYPE_START_POSITION = 10;
        private const int FILTER_METHOD_START_POSITION = 11;
        private const int INTERLACE_START_POSITION = 12;

        public int Width { get; set; }

        public int Length { get; set; }

        public byte BitDepth { get; set; }

        public byte ColourType { get; set; }

        public byte CompressionType { get; set; }

        public byte FilterMethod { get; set; }

        public byte Interlace { get; set; }

        public byte[] IDATdata { get; set; }

        public Pixel[] Plte { get; set; }

        private int rgbNumber { get; set; }
        private byte[] alphaArr { get; set; }
        private int lineSize { get; set; }
        public byte[] RGBArray { get; set; }



        public Image Read(string filePath)
        {
            ReadBytesFromFile(filePath);
            IsFilePng();
            GetChunks();

            Image image = new Image(Width, Length, RGBArray);
            return image;
        }

        private void GetChunks()
        {
            int pos = NUMBER_DEFINITION_FORMAT_BYTES;
            while (pos < inputBytes.Length)
            {
                
                PngChunk chunk = new PngChunk(inputBytes.Skip(pos).ToArray());

                Console.WriteLine($"chunk data length {chunk.DataLength}");
                Console.WriteLine($"chunk type {chunk.Type}");
                Console.WriteLine($"chunk total length {chunk.TotalLength}");

                Console.WriteLine();


                switch (chunk.Type)
                {
                    case IHDR:
                        ParseIHDR(chunk);
                        break;
                    case IDAT:
                        AddIDAT(chunk);
                        break;
                    case PLTE:
                        ParsePLTE(chunk);
                        break;
                    case IEND:
                        ParseIDAT(chunk);
                        break;
                    default:
                        throw new Exception("Program work only with image, which consists of critical chunks");
                }

                pos += chunk.TotalLength;
            }
        }

        private void ParseIDAT(PngChunk chunk)
        {
            byte[] decodedSequence = Inflate(IDATdata);
            rgbNumber = Width * Length * 3;
            lineSize = Width * 3;

            if (ColourType == 6)
                decodedSequence = GetAlpha(decodedSequence);

            if (Plte.Length != 0)                                       //color type 3
                decodedSequence = UsePLTE(decodedSequence);

            RGBArray = new byte[rgbNumber];
            UseFilters(decodedSequence);

            if (ColourType == 6)
                UseAlphaChannel();


        }

        private void UseAlphaChannel()
        {
            for (int i = 0, j = 0; i < RGBArray.Length; i += 3, j++)
            {
                float alpha = 1 - (alphaArr[j] / 255);
                RGBArray[i] = (byte)Math.Round(((double)alpha * ((double)RGBArray[i] / 255) + ((1 - alpha) * (255 / 255))) * 255);
                RGBArray[i + 1] = (byte)Math.Round(((double)alpha * ((double)RGBArray[i + 1] / 255) + (1 - alpha)) * 255);
                RGBArray[i + 2] = (byte)Math.Round(((double)alpha * ((double)RGBArray[i + 2] / 255) + (1 - alpha)) * 255);
            }
        }

        private void UseFilters(byte[] decodedSequence)
        {
            int point = 0;
            int pos = 0;
            while (pos < decodedSequence.Length)
            {

                int filter = decodedSequence[pos];
                switch (filter)
                {
                    case 0:
                        UseNoneFilter(decodedSequence, pos, ref point);
                        break;
                    case 1:
                        UseSubFilter(decodedSequence, pos, ref point);
                        break;
                    case 2:
                        UseUpFilter(decodedSequence, pos, ref point);
                        break;
                    case 3:
                        UseAverageFilter(decodedSequence, pos, point: ref point);
                        break;
                    case 4:
                        UsePaethFilter(decodedSequence, pos, ref point);
                        break;
                    default:
                        break;

                };
                pos += (lineSize + 1);

            }
        }

        private void UsePaethFilter(byte[] decodedSequence, int currPos, ref int point)
        {
            byte[] previousLeftCurrent = new byte[3];
            byte[] previousPixelsLine = GetSubSequence(RGBArray, point - lineSize);
            byte[] currentPixelsLine = GetSubSequence(decodedSequence, currPos + 1);

            byte[] filteredArr = new byte[lineSize];

            for (int i = 0; i < currentPixelsLine.Length; i++)
            {
                int previousLeftPreviousLine = i - 3 > 0 ? previousPixelsLine[i - 3] : 0;
                int p = previousLeftCurrent[i % 3] + previousPixelsLine[i] - previousLeftPreviousLine;
                int pa = Math.Abs(p - previousLeftCurrent[i % 3]);
                int pb = Math.Abs(p - previousPixelsLine[i]);
                int pc = Math.Abs(p - previousLeftPreviousLine);

                int current = currentPixelsLine[i];
                if (pa <= pb && pa <= pc)
                    current += previousLeftCurrent[i % 3];
                else if (pb <= pc)
                    current += previousPixelsLine[i];
                else
                    current += previousLeftPreviousLine;

                previousLeftCurrent[i % 3] = (byte)current;
                filteredArr[i] = (byte)current;
            }

            filteredArr.CopyTo(RGBArray, point);
            point += lineSize;
        }

        private void UseAverageFilter(byte[] decodedSequence, int currPos, ref int point)
        {
            byte[] leftPrevious = new byte[3];
            byte[] previousPixelsLine = GetSubSequence(RGBArray, point - lineSize);
            byte[] currentPixelsLine = GetSubSequence(decodedSequence, currPos + 1);

            byte[] filteredArr = new byte[lineSize];


            for (int i = 0; i < currentPixelsLine.Length; i++)
            {
                byte current = (byte)((currentPixelsLine[i] +
                    Math.Floor(((double)leftPrevious[i % 3] + (double)previousPixelsLine[i]) / 2)) % 256);

                leftPrevious[i % 3] = current;
                filteredArr[i] = current;
            }



            // int rgbaArrayPos = defArrayStartPointer(currPos);
            filteredArr.CopyTo(RGBArray, point);
            point += lineSize;
        }

        private void UseUpFilter(byte[] decodedSequence, int currPos, ref int point)
        {
            byte[] previousPixelsLine = GetSubSequence(RGBArray, point - lineSize);
            byte[] currentPixelsLine = GetSubSequence(decodedSequence, currPos + 1);
            byte[] filteredArr = new byte[lineSize];

            for (int i = 0; i < lineSize; i++)
                filteredArr[i] = (byte)((previousPixelsLine[i] + currentPixelsLine[i]) % 256);


            filteredArr.CopyTo(RGBArray, point);
            point += lineSize;


            Console.WriteLine();
        }

        private void UseSubFilter(byte[] decodedSequence, int currPos, ref int point)
        {
            byte[] previous = new byte[3];
            byte[] currentPixelsArray = GetSubSequence(decodedSequence, currPos + 1);
            byte[] filteredArr = new byte[lineSize];
            int pointer = 0;


            while (pointer < currentPixelsArray.Length)
            {
                byte r = (byte)((currentPixelsArray[pointer] + previous[0]) % 256);
                filteredArr[pointer] = r;
                previous[0] = r;
                pointer++;
                byte g = (byte)((currentPixelsArray[pointer] + previous[1]) % 256);
                filteredArr[pointer] = g;
                previous[1] = g;
                pointer++;
                byte b = (byte)((currentPixelsArray[pointer] + previous[2]) % 256);
                filteredArr[pointer] = b;
                previous[2] = b;
                pointer++;
            }


            //int rgbaArrayPos = defArrayStartPointer(currPos);
            filteredArr.CopyTo(RGBArray, point);
            point += lineSize;
        }

        private void UseNoneFilter(byte[] decodedSequence, int currPos, ref int point)
        {
            GetSubSequence(decodedSequence, currPos + 1).CopyTo(RGBArray, point);
            point += lineSize;
        }

        private byte[] GetSubSequence(byte[] decodedSequence, int skip)
        {
            return decodedSequence.Skip(skip).Take(lineSize).ToArray();
        }

        private byte[] UsePLTE(byte[] decodedSequence)
        {
            byte[] output = new byte[(rgbNumber) + Length];

            int pointerDS = 0;
            int pointerOutput = 0;
            int pointerTrns = 0;

            while (pointerDS < decodedSequence.Length)
            {

                output[pointerOutput] = decodedSequence[pointerDS];
                pointerDS++;
                pointerOutput++;
                for (int i = pointerOutput, j = pointerDS; i < pointerOutput + lineSize; i += 3, j++)
                {


                    Pixel pixel = Plte[decodedSequence[j]];

                    output[i] = pixel.r;
                    output[i + 1] = pixel.g;
                    output[i + 2] = pixel.b;


                }
                pointerOutput += lineSize;
                pointerDS += Width;
                pointerTrns += Width;
            }

            return output;
        }


        private byte[] GetAlpha(byte[] decodedSequence)
        {
            alphaArr = new byte[rgbNumber];
            byte[] output = new byte[rgbNumber + Length];


            int pointerDS = 0;
            int pointerOutput = 0;

            while (pointerDS < decodedSequence.Length)
            {

                output[pointerOutput] = decodedSequence[pointerDS];
                pointerDS++;
                pointerOutput++;
                for (int i = pointerOutput, j = pointerDS, k = 0; i < pointerOutput + lineSize; i += 3, j += 4, k++)
                {
                    output[i] = decodedSequence[j];
                    output[i + 1] = decodedSequence[j + 1];
                    output[i + 2] = decodedSequence[j + 2];

                    alphaArr[k] = decodedSequence[j + 3];

                }
                pointerOutput += lineSize;
                pointerDS += (Width * 4);
            }


            return output;
        }

        private byte[] Inflate(byte[] inputByte)
        {
            byte[] temp = new byte[1024];
            MemoryStream memory = new MemoryStream();
            ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
            inf.SetInput(inputByte);
            while (!inf.IsFinished)
            {
                int extracted = inf.Inflate(temp);
                if (extracted > 0)
                {
                    memory.Write(temp, 0, extracted);
                }
                else
                {
                    break;
                }
            }
            return memory.ToArray();
        }


        private void ParsePLTE(PngChunk chunk)
        {
            Plte = new Pixel[chunk.DataLength];
            for (int i = 0, j = 0; i < chunk.DataLength; i += 3, j++)
            {
                Plte[j] = new Pixel(chunk.Data[i], chunk.Data[i + 1], chunk.Data[i + 2]);
            }
        }

        
        private void AddIDAT(PngChunk chunk)
        {
            int index = IDATdata.Length;
            IDATdata = new byte[IDATdata.Length + chunk.DataLength];
            chunk.Data.CopyTo(IDATdata, index);
        }

        
        private void ParseIHDR(PngChunk chunk)
        {
            Width = GetDataFromBytesArray(WIDTH_START_POSITION, FOUR_BYTES_BLOCK, chunk.Data);
            Length = GetDataFromBytesArray(LENGTH_START_POSITION, FOUR_BYTES_BLOCK, chunk.Data);
            BitDepth = (byte)GetDataFromBytesArray(BIT_DEPTH_START_POSITION, ONE_BYTE_BLOCK, chunk.Data);
            ColourType = (byte)GetDataFromBytesArray(COLOR_TYPE_START_POSITION, ONE_BYTE_BLOCK, chunk.Data);
            CompressionType = (byte)GetDataFromBytesArray(COMPRESSION_TYPE_START_POSITION, ONE_BYTE_BLOCK, chunk.Data);
            FilterMethod = (byte)GetDataFromBytesArray(FILTER_METHOD_START_POSITION, ONE_BYTE_BLOCK, chunk.Data);
            Interlace = (byte)GetDataFromBytesArray(INTERLACE_START_POSITION, ONE_BYTE_BLOCK, chunk.Data);

            IDATdata = new byte[0];
            Plte = new Pixel[0];

            if (Width <= 0 || Length <= 0 || FilterMethod != 0 || CompressionType != 0)
                throw new Exception("Image is incorrect. Converting is not available");
/*            Console.WriteLine(Width);
            Console.WriteLine(Length);
            Console.WriteLine(BitDepth);
            Console.WriteLine(ColourType);
            Console.WriteLine(CompressionType);
            Console.WriteLine($"filter method {FilterMethod}");
            Console.WriteLine(Interlace);*/
        }

       
        private int GetDataFromBytesArray(int start, int count, byte[] bytes)
        {
            string currHexValue = "";
            for (int i = start; i < start + count; i++)
                currHexValue += bytes[i].ToString("X");
            return Convert.ToInt32(currHexValue, 16);
        }

        
        private void IsFilePng()
        {
            string currentPngDefinitionBytes = "";

            for (int i = 0; i < NUMBER_DEFINITION_FORMAT_BYTES; i++)
                currentPngDefinitionBytes += inputBytes[i].ToString("X");

            if (currentPngDefinitionBytes != PNG_DEFINED_BYTES)
                throw new Exception("This file isn't png");
        }


        private void ReadBytesFromFile(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            inputBytes = new byte[file.Length];
          
            for (int i = 0; i < file.Length; i++)
                inputBytes[i] = (byte)file.ReadByte();
        }
    }
}
