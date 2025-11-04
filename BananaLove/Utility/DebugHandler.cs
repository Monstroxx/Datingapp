using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BananaLove.Utility
{
    class DebugHandler
    {
        private static bool isDebugMode = true;
        public static void Log(string message)
        {

            if (isDebugMode)
            {
                string debugMessage = $"[DEBUG] {DateTime.Now}: {message}";
                Console.WriteLine(debugMessage);
                //write to a log file
                using (StreamWriter writer = new StreamWriter("debug.log", true))
                {
                    writer.WriteLine(debugMessage);
                }
            }
        }
    }
}
