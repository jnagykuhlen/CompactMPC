using System;
using System.Collections.Generic;
using System.Text;

namespace CompactMPC.Networking
{
    public class NetworkConsistencyException : Exception
    {
        public NetworkConsistencyException(string message) : base(message) { }
        public NetworkConsistencyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
