using System;

namespace main.Exceptions
{
    public class UnableToConnectToServerException : Exception
    {
        public UnableToConnectToServerException(string ipPort) : base($"Failed to connect to server {ipPort}")
        {
        }
    }
}