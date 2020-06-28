using System;

namespace main.Exceptions
{
    /// <summary>
    /// Raised exception when a socket connection fails due to a data issue.
    /// </summary>
    public class InvalidSocketDataException : Exception
    {
        public InvalidSocketDataException(string reason) : base($"Invalid socket data: {reason}")
        {
        }
    }
}
