using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class CircuitEvaluation<T>
    {
        private ICircuitEvaluator<T> _evaluator;
        private Dictionary<Wire, T> _wireValues;
        private bool _hasInputChanged;

        public CircuitEvaluation(ICircuitEvaluator<T> evaluator)
        {
            _evaluator = evaluator;
            _wireValues = new Dictionary<Wire, T>();
            _hasInputChanged = false;
        }

        public T GetWireValue(Wire wire)
        {
            if (_hasInputChanged)
            {
                ClearCachedWireValues();
                _hasInputChanged = false;
            }

            T value;
            if (!_wireValues.TryGetValue(wire, out value))
            {
                if (wire.IsConstant)
                    throw new ArgumentException("Cannot get value of constant wire.", nameof(wire));

                if (wire.IsAssignable)
                    throw new ArgumentException("Cannot get value of unassigned input wire.", nameof(wire));


                value = wire.Gate.Evaluate(_evaluator, this);
                _wireValues[wire] = value;
            }

            return value;
        }

        public void AssignInputValue(Wire wire, T value)
        {
            if (!wire.IsAssignable)
                throw new ArgumentException("Values can only be assigned to input wires.", nameof(wire));

            _wireValues[wire] = value;
            _hasInputChanged = true;
        }

        private void ClearCachedWireValues()
        {
            _wireValues = _wireValues.Where(pair => pair.Key.IsAssignable).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
