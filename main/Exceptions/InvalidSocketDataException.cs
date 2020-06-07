using System;

namespace main.Exceptions
{
    public class InvalidSocketDataException : Exception
    {
        public InvalidSocketDataException(string reason) : base($"Invalid socket data: {reason}")
        {
        }
    }
}