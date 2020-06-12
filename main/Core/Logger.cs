using System;

namespace main.Core
{
    public static class Logger
    {
        public static void Write(string s)
        {
            Console.WriteLine($"[{DateTime.UtcNow.ToString()}] {s}");
        }
    }
}
