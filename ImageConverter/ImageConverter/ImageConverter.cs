using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ImageConverter
{
    public class ImageConverter
    {

        private string sourcePath;
        private string sourceFileFormat;
        private string goalFormat;

        private const string PPM_FILE_FORMAT = "ppm";
        private const string PNG_FILE_FORMAT = "png";
        private const string JPEG_FILE_FORMAT = "ppm";


        private List<string> availableSourceFormats = new List<string>{ PNG_FILE_FORMAT };    
        private List<string> availableGoalFormats = new List<string> { JPEG_FILE_FORMAT, PPM_FILE_FORMAT };



        public ImageConverter(string[] args)
        {
            ParseInputLine(args);
        }

        private void ParseInputLine(string[] args)
        {
            if (!IsCorrectAttrs(args))
                throw new Exception("Incorrect response. Right format: --source=[file-path] --goal-format=[file-format]");
            GetSourcePath(args[0]);
            GetSourceFileFormat(args[0]);
            GetGoalFormat(args[1]);
        }

        private void GetSourceFileFormat(string source)
        {
            sourceFileFormat = source.Substring(source.IndexOf('.')+1);
        }

        private void GetGoalFormat(string goal)
        {
            goalFormat = goal.Substring(14);
        }

        private void GetSourcePath(string source)
        {
            sourcePath = source.Substring(9);
        }

        private bool IsCorrectAttrs(string[] args)
        {
            Regex sourceRegex = new Regex(@"--source=(\S+)\.(\w+)");
            Regex goalRegex = new Regex(@"--goal-format=(\w+)");

            if (sourceRegex.IsMatch(args[0]) && goalRegex.IsMatch(args[1]))
                return true;

            return false;
        }

        internal void Convert()
        {
            IImageReader reader;
            Console.WriteLine(sourcePath);
            switch (sourceFileFormat)
            {
                case PNG_FILE_FORMAT:
                    reader = new PngReader();
                    break;
                default:
                    throw new Exception("Program working only with png");

            }

            reader.Read(sourcePath);
        }


    }
}
