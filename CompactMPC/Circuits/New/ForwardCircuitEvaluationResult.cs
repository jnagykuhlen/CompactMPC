using System.Collections.Generic;

namespace CompactMPC.Circuits.New
{
    public class ForwardCircuitEvaluationResult<T>
    {
        private readonly IReadOnlyDictionary<Wire, T> _valuesByWire;
        
        public ForwardCircuitEvaluationResult(IReadOnlyDictionary<Wire, T> valuesByWire)
        {
            _valuesByWire = valuesByWire;
        }

        public T Value(Wire wire)
        {
            if (_valuesByWire.TryGetValue(wire, out T value))
                return value;

            throw new CircuitEvaluationException("Wire value was not evaluated.");
        }

        public ForwardCircuitEvaluationResult<T> Value(Wire wire, out T value)
        {
            value = Value(wire);
            return this;
        }
            
        public IReadOnlyDictionary<Wire, T> ToDictionary()
        {
            return _valuesByWire;
        }
    }
}