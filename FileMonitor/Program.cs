using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = ConfigurationManager.AppSettings[""];
            var dirInfo = new DirectoryInfo("");

            Console.ReadKey();
        }
    }
}
