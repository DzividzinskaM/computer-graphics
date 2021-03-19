using System;
using System.Collections.Generic;
using System.Text;

namespace ImageConverter
{

    public class PngChunk
    {
        public int TotalLength { get; set; }

        public int DataLength { get; set; }

        public string Type { get; set; }

        public byte[] Data { get; set; }

        public byte[] CRC { get; set; }
        const int CHUNK_UNIT_LENGTH = 4;


        public PngChunk(byte[] bytes)
        {
            GetDataLength(bytes);
            GetChunkType(bytes);
            GetData(bytes);
            GetCrc(bytes);
            TotalLength = DataLength + (CHUNK_UNIT_LENGTH*3);

        }

        
        private void GetCrc(byte[] bytes)
        {
            CRC = new byte[4];
            for (int i = (CHUNK_UNIT_LENGTH*2) + DataLength, j = 0; i < DataLength + (CHUNK_UNIT_LENGTH*3); i++, j++)
                CRC[j] = bytes[i];

        }

        private void GetData(byte[] bytes)
        {
            Data = new byte[DataLength];
            for (int i = CHUNK_UNIT_LENGTH*2, j = 0; i < (CHUNK_UNIT_LENGTH*2) + DataLength; i++, j++)
                Data[j] = bytes[i];
        }

        private void GetChunkType(byte[] bytes)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            Type = ascii.GetString(bytes, CHUNK_UNIT_LENGTH, CHUNK_UNIT_LENGTH);

        }

        private void GetDataLength(byte[] bytes)
        {
            string dataLengthHex = "";

            for (int i = 0; i < CHUNK_UNIT_LENGTH; i++)
                dataLengthHex += bytes[i].ToString("X");

            DataLength = Convert.ToInt32(dataLengthHex, 16);

        }
    }
}
