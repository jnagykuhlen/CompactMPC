using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public class DesynchronizationException : Exception
    {
        public DesynchronizationException(string message) : base(message) { }
        public DesynchronizationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
