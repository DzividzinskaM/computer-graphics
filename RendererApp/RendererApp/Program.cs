using System;


namespace RendererApp
{
    class Program
    {
        
        static void Main(string[] args)
        {
            try
            {
                RendererApp app = new RendererApp();
                app.Start(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
