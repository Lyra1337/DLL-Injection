using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DllInjection;

namespace InjectionTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);

            var process = Process.GetProcessesByName("TestAppHost").Single();
            var dll = String.Concat(Environment.CurrentDirectory, @"\..\..\..\..\ManagedMessageBox\bin\x64\Debug\ManagedMessageBox.dll");
            //var dll = Environment.CurrentDirectory + "\\ManagedMessageBox.dll";

            if (File.Exists(dll) == false)
            {
                throw new FileNotFoundException();
            }

            Injector.Inject(process.Id, dll);
        }
    }
}
