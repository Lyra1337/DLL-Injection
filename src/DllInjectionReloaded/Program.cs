using Reloaded.Injector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllInjectionReloaded
{
    class Program
    {
        static void Main(string[] args)
        {
            var process = Process.GetProcessesByName("TestAppHost").Single();
            var dll = new FileInfo(String.Concat(Environment.CurrentDirectory, @"\..\..\..\..\ManagedMessageBox\bin\x64\Debug\ManagedMessageBox.dll"));

            if (dll.Exists == false)
            {
                throw new FileNotFoundException();
            }

            using (var injector = new Injector(process))
            {
                var result = injector.Inject(dll.FullName);
                injector.CallFunction(dll.FullName, "Initialize", 0);
            }
        }
    }
}
