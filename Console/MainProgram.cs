using System;

namespace Console
{
    public class MainProgram
    {
        
        public static void Main(string[] args)
        {
            var settings = new Settings();

            try
            {
                var watcher = new Watcher(settings, args);
                watcher.Init();
            }
            catch (UsageException e)
            {
                System.Console.WriteLine(settings.GetUsage().Replace("<<ERRORMSG>>", e.Message));
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

        }

    }

}
