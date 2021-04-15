using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RendererApp
{
    public class RendererApp
    {

        private string sourcePath;
        private string outputPath;


        public void Start(string[] args)
        {
            ParseInputLine(args);
            Renderer renderer = new Renderer(width: 300, angelX:4.7124);
            renderer.Render(sourcePath, outputPath);
        }

        private void ParseInputLine(string[] args)
        {
            if (args.Length == 0)
                throw new Exception("Right format: Renderer.exe --source=[file-path] --output=[file-format]");
            if (!IsCorrectAttrs(args))
                throw new Exception("Incorrect response. Right format: --source=[file-path] --output=[file-format]");
            GetSourcePath(args[0]);
            GetGoalPath(args[1]);
        }

        private void GetGoalPath(string output)
        {
            outputPath = output.Substring(9);
        }

        private void GetSourcePath(string source)
        {
            sourcePath = source.Substring(9);
        }

        private bool IsCorrectAttrs(string[] args)
        {
            var sourceRegex = new Regex(@"--source=(\S+)\.(\w+)");
            Regex goalRegex = new Regex(@"--output=(\S+)\.(\w+)");

            if (sourceRegex.IsMatch(args[0]) && goalRegex.IsMatch(args[1]))
                return true;

            return false;
        }
    }
}
