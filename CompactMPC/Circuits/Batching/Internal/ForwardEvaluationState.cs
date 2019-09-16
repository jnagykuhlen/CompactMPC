using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardEvaluationState<T>
    {
        private T[] _output;
        private Dictionary<ForwardGate, T> _cachedInputValueByGate;
        private Queue<GateEvaluation<T>> _delayedAndGateEvaluations;

        public ForwardEvaluationState(int numberOfOutputGates)
        {
            _output = new T[numberOfOutputGates];
            _cachedInputValueByGate = new Dictionary<ForwardGate, T>();
            _delayedAndGateEvaluations = new Queue<GateEvaluation<T>>();
        }
        
        public void SetOutput(int index, T value)
        {
            _output[index] = value;
        }

        public void WriteInputValueToCache(ForwardGate gate, T value)
        {
            try
            {
                _cachedInputValueByGate.Add(gate, value);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException("Another input value is already present.", exception);
            }
        }

        public Optional<T> ReadInputValueFromCache(ForwardGate gate)
        {
            T value;
            if (_cachedInputValueByGate.TryGetValue(gate, out value))
            {
                _cachedInputValueByGate.Remove(gate);
                return Optional<T>.FromValue(value);
            }

            return Optional<T>.Empty;
        }
        
        public void DelayAndGateEvaluation(GateEvaluation<T> evaluation)
        {
            _delayedAndGateEvaluations.Enqueue(evaluation);
        }

        public GateEvaluation<T>[] GetDelayedAndGateEvaluations()
        {
            GateEvaluation<T>[] nextDelayedAndGateEvaluations = _delayedAndGateEvaluations.ToArray();
            _delayedAndGateEvaluations.Clear();
            return nextDelayedAndGateEvaluations;
        }
        
        public T[] Output
        {
            get
            {
                return _output;
            }
        }
    }
}
