using System;

namespace main.Exceptions
{
    public class InvalidIpParseException : Exception
    {
        public InvalidIpParseException(string reason) : base($"Unable to parse Ip address: {reason}")
        {
        }
    }
}