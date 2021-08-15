using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngine
{
    public static class Logger
    {
        public static Task LogThread;
        public static void Write(string message)
        {
                Console.WriteLine(message);
        }
    }
}
