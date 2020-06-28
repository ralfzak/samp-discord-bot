using System;

namespace main.Exceptions
{
    /// <summary>
    /// Raised exception when a wiki page is fails parsing.
    /// </summary>
    public class InvalidWikiPageException : Exception
    {
        public InvalidWikiPageException(string reason) : base(reason)
        {
        }
    }
}
