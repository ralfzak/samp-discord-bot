using System;

namespace main.Exceptions
{
    public class InvalidWikiPageException : Exception
    {
        public InvalidWikiPageException(string reason) : base(reason)
        {
        }
    }
}
