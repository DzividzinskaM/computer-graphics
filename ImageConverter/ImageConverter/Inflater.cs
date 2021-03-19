using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;

namespace ImageConverter
{
    public class Inflater
    {
        public static byte[] Inflate(byte[] inputByte)
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
    }
}
