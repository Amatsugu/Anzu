using System;
using Nancy.Hosting.Self;

namespace Anzu
{
    class Program
    {
        static void Main(string[] args)
        {
			var host = new NancyHost(new Uri("http://localhost:1234"));
			host.Start();
			Console.Write($"Hosting...");
			Console.ReadLine();
        }
    }
}
