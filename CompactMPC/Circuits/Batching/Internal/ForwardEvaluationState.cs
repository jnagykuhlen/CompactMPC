using System;
using System.Collections.Generic;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardEvaluationState<T>
    {
        private readonly Dictionary<ForwardGate, T> _cachedInputValueByGate;
        private readonly Queue<GateEvaluation<T>> _delayedAndGateEvaluations;

        public T[] Output { get; }
        
        public ForwardEvaluationState(int numberOfOutputGates)
        {
            _cachedInputValueByGate = new Dictionary<ForwardGate, T>();
            _delayedAndGateEvaluations = new Queue<GateEvaluation<T>>();
            Output = new T[numberOfOutputGates];
        }
        
        public void SetOutput(int index, T value)
        {
            Output[index] = value;
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
            if (_cachedInputValueByGate.TryGetValue(gate, out T value))
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
    }
}
