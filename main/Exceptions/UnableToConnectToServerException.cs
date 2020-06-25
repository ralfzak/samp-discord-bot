using System;

namespace main.Exceptions
{
    /// <summary>
    /// Raised exception when a SAMP server connection fails.
    /// </summary>
    public class UnableToConnectToServerException : Exception
    {
        public UnableToConnectToServerException(string ipPort) : base($"Failed to connect to server {ipPort}")
        {
        }
    }
}
