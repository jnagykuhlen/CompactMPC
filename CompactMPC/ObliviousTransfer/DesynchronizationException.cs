using System;

namespace CompactMPC.ObliviousTransfer
{
    public class DesynchronizationException : Exception
    {
        public DesynchronizationException(string message) : base(message) { }
        public DesynchronizationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
