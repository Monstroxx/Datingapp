using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BananaLove.Utility
{
    class DebugHandler
    {
        private static bool isDebugMode = true;
        
        static string time {get{ return DateTime.Now.ToString("HH:mm:ss.fff"); }}
public static void Log(string message)
        {
            if (isDebugMode)
            {
                TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
                string debugMessage = $"[DEBUG] {time}: {message}";
                Console.WriteLine(debugMessage);
                //write to a log file
                using (StreamWriter writer = new StreamWriter("debug.log", true))
                {
                    writer.WriteLine(debugMessage);
                }
            }
        }
        public static void LogError(string message)
        {
            if (isDebugMode)
            {
                string errorMessage = $"[ERROR] {time}: {message}";
                Console.WriteLine(errorMessage);
                //write to a log file
                using (StreamWriter writer = new StreamWriter("debug.log", true))
                {
                    writer.WriteLine(errorMessage);
                }
            }
        }
        public static void seperate()
        {
            if (isDebugMode)
            {
                using (StreamWriter writer = new StreamWriter("debug.log", true))
                {
                    writer.WriteLine("=================================================================================");
                }
            }
        }
    }
}
