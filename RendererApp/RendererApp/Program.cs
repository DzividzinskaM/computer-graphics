using System;


namespace RendererApp
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //long start = DateTime.Now.Ticks;
            try
            {
                RendererApp app = new RendererApp();
                app.Start(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

          /*  DateTime currentDate = DateTime.Now;
            long elapsedTicks = currentDate.Ticks - start;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
            Console.WriteLine("Потрачено тактов на выполнение: " + elapsedSpan.TotalSeconds);*/
        }
    }
}
