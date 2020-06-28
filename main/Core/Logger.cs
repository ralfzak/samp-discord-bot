using System;

namespace main.Core
{
    /// <summary>
    /// Maintains application logs.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Logs a given <paramref name="message"/>. 
        /// </summary>
        /// <param name="message">A string to log</param>
        public static void Write(string message)
        {
            Console.WriteLine($"[{DateTime.UtcNow.ToString()}] {message}");
        }
    }
}
