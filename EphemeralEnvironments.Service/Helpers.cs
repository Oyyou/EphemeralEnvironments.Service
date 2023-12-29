using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EphemeralEnvironments.Service
{
    public static class Helpers
    {
        public static void Log(string value)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {value}");
        }
    }
}
