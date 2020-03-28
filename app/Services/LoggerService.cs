using System;

namespace app.Services
{
    class LoggerService
    {
        public static void Write(string s)
        {
            Console.WriteLine($"[{DateTime.UtcNow.ToString()}] {s}");
        }
    }
}
