using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAppHost
{
    public sealed class Program
    {
        public static Program Instance { get; } = new Program();

        private async static Task Main()
        {
            await Program.Instance.Run();
        }

        private bool run = true;

        private async Task Run()
        {
            while (this.run == true)
            {
                Console.WriteLine("I'm running!");

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    Console.WriteLine(assembly.FullName);
                }

                await Task.Delay(1000);
            }

            Console.WriteLine("I'm done!");
            Console.ReadKey();
        }
    }
}
