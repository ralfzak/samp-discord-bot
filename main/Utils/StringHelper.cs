using System;
using System.Linq;

namespace main.Utils
{
    /// <summary>
    /// Encapsulates a set of string functions.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Calculates the Levenshtein distance of two given strings.
        /// More information on the algorithm here: https://en.wikipedia.org/wiki/Levenshtein_distance
        /// </summary>
        /// <param name="s">A string sequence to compare</param>
        /// <param name="t">A string sequence to compare</param>
        /// <returns>the Levenshtein distance</returns>
        public static int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;

            if (m == 0)
                return n;

            for (int i = 0; i <= n; d[i, 0] = i++) {}
            for (int j = 0; j <= m; d[0, j] = j++) {}

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            
            return d[n, m];
        }
        
        /// <summary>
        /// Generates a random string of a specified <paramref name="length"/>.
        /// </summary>
        /// <param name="length">Length of the random generated string</param>
        /// <returns>A random string</returns>
        public static string GenerateRandom(int length)
        {
            Random random = new Random();
            return 
                new string(
                    Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length)
                        .Select(s => s[random.Next(s.Length)])
                        .ToArray()
                );
        }
    }
}
