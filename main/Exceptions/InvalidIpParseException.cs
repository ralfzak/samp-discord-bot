using System;

namespace main.Exceptions
{
    /// <summary>
    /// Raised exception when a given IP fails to be parsed.
    /// </summary>
    public class InvalidIpParseException : Exception
    {
        public InvalidIpParseException(string reason) : base($"Unable to parse Ip address: {reason}")
        {
        }
    }
}
