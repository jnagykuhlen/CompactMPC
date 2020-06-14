using System;

namespace CompactMPC.Circuits
{
    public class CircuitEvaluationException : Exception
    {
        public CircuitEvaluationException(string message) : base(message) { }
        public CircuitEvaluationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
