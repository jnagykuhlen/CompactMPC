using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardEvaluationState<T>
    {
        private readonly Dictionary<ForwardGate, T> _cachedInputValueByGate;
        private readonly Queue<GateEvaluation<T>> _delayedAndGateEvaluations;
        private readonly Dictionary<ForwardGate, int> _outputIndexByGate;
        private readonly Optional<T>[] _output;

        public ForwardEvaluationState(IReadOnlyList<ForwardGate> outputGates)
        {
            _cachedInputValueByGate = new Dictionary<ForwardGate, T>();
            _delayedAndGateEvaluations = new Queue<GateEvaluation<T>>();
            _outputIndexByGate = Enumerable
                .Range(0, outputGates.Count)
                .ToDictionary(i => outputGates[i]);

            _output = new Optional<T>[outputGates.Count];
        }

        public void SetOutputValue(ForwardGate gate, T value)
        {
            if (_outputIndexByGate.TryGetValue(gate, out int index))
                _output[index] = Optional<T>.FromValue(value);
        }

        public void WriteInputValueToCache(ForwardGate gate, T value)
        {
            try
            {
                _cachedInputValueByGate.Add(gate, value);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException("Another cached input value is already present.", exception);
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

        public GateEvaluation<T>[] NextDelayedAndGateEvaluations()
        {
            GateEvaluation<T>[] nextDelayedAndGateEvaluations = _delayedAndGateEvaluations.ToArray();
            _delayedAndGateEvaluations.Clear();
            return nextDelayedAndGateEvaluations;
        }

        public IReadOnlyList<Optional<T>> Output
        {
            get
            {
                return _output;
            }
        }
    }
}
