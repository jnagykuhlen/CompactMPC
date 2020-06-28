using System;
using System.Collections.Generic;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardEvaluationState<T>
    {
        private readonly HashSet<ForwardGate> _outputGates;
        private readonly Dictionary<ForwardGate, T> _outputValuesByGate;
        private readonly Dictionary<ForwardGate, T> _cachedInputValuesByGate;
        private readonly Queue<GateEvaluation<T>> _delayedAndGateEvaluations;

        public ForwardEvaluationState(IEnumerable<ForwardGate> outputGates)
        {
            _outputGates = new HashSet<ForwardGate>(outputGates);
            _outputValuesByGate = new Dictionary<ForwardGate, T>(_outputGates.Count);
            _cachedInputValuesByGate = new Dictionary<ForwardGate, T>();
            _delayedAndGateEvaluations = new Queue<GateEvaluation<T>>();
        }

        public void SetOutputValue(ForwardGate gate, T value)
        {
            if (_outputGates.Contains(gate))
                _outputValuesByGate.Add(gate, value);
        }

        public bool GetOutputValue(ForwardGate gate, out T value)
        {
            return _outputValuesByGate.TryGetValue(gate, out value);
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
    }
}
