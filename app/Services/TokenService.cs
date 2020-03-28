using System;
using System.Linq;

namespace app.Services
{
    public static class TokenService
    {
        public static string Generate(int length)
        {
            Random random = new Random();

            return 
                new string(
                    Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", length)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray()
                );
        }
    }
}
