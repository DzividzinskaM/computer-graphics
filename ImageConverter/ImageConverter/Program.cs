using System;

namespace ImageConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ImageConverter imageConverter = new ImageConverter(args);
                imageConverter.Convert();
            }
            catch(Exception error)
            {
                Console.WriteLine(error.Message);
            }           
        }
    }
}
