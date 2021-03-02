using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllInjection
{
    static class App
    {
        static void Main(string[] args)
        {
            // Check arguments
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Usage: DllInjection.exe <target process id> <path to dll>");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }

            int ProcId = int.Parse(args[0]);
            string DllPath = args[1];

            // Make sure file exist
            if (File.Exists(DllPath) == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] File {0} does not exist. Please check file path.", DllPath);
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }

            Injector.Inject(ProcId, DllPath);
        }
    }
}
