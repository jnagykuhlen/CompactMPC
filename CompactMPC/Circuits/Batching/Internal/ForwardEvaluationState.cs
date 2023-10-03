using System;
using System.Collections.Generic;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardEvaluationState<T>
    {
        private readonly Dictionary<ForwardGate, T> _cachedInputValuesByGate = new Dictionary<ForwardGate, T>();
        private readonly Queue<GateEvaluation<T>> _delayedAndGateEvaluations = new Queue<GateEvaluation<T>>();

        public void SetOutputValue(ForwardGate gate, T value)
        {
            OnOutputEvaluated?.Invoke(gate, value);
        }

        public void WriteInputValueToCache(ForwardGate gate, T value)
        {
            try
            {
                _cachedInputValuesByGate.Add(gate, value);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException("Another cached input value is already present.", exception);
            }
        }

        public bool ReadInputValueFromCache(ForwardGate gate, out T value)
        {
            if (_cachedInputValuesByGate.TryGetValue(gate, out value))
            {
                _cachedInputValuesByGate.Remove(gate);
                return true;
            }

            return false;
        }

        public void DelayAndGateEvaluation(GateEvaluation<T> evaluation)
        {
            _delayedAndGateEvaluations.Enqueue(evaluation);
        }

        public GateEvaluation<T>[] NextDelayedAndGateEvaluations()
        {
            GateEvaluation<T>[] nextDelayedAndGateEvaluations = _delayedAndGateEvaluations.ToArray();
            _delayedAndGateEvaluations.Clear();
            return nextDelayedAndGateEvaluations;
        }

        public event OutputEvaluatedHandler? OnOutputEvaluated;
        
        public delegate void OutputEvaluatedHandler(ForwardGate gate, T value);
    }
}
