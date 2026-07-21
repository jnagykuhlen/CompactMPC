using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CompactMPC.Circuits.Internal;

public class ForwardEvaluationState<T>(IEnumerable<ForwardGate> outputGates)
{
    private readonly Dictionary<ForwardGate, T> _cachedInputValuesByGate = new();
    private readonly Queue<GateEvaluation<T>> _delayedAndGateEvaluations = new();

    private readonly Dictionary<ForwardGate, OptionalOutputValue> _outputValuesByGate =
        outputGates.ToDictionary(gate => gate, _ => new OptionalOutputValue());

    public void SetOutputValue(ForwardGate gate, T value)
    {
        if (_outputValuesByGate.TryGetValue(gate, out var optionalOutputValue))
            optionalOutputValue.Value = value;
    }

    public T GetOutputValue(ForwardGate gate)
    {
        if (!_outputValuesByGate.TryGetValue(gate, out var optionalOutputValue))
            throw new ArgumentException("The given gate is not an output gate.", nameof(gate));
        
        return optionalOutputValue.Value;
    }

    public void WriteInputValueToCache(ForwardGate gate, T value)
    {
        if (!_cachedInputValuesByGate.TryAdd(gate, value))
            throw new InvalidOperationException("Another cached input value is already present.");
    }

    public bool ReadInputValueFromCache(ForwardGate gate, [MaybeNullWhen(false)] out T value) =>
        _cachedInputValuesByGate.Remove(gate, out value);

    public void DelayAndGateEvaluation(GateEvaluation<T> evaluation) => _delayedAndGateEvaluations.Enqueue(evaluation);

    public GateEvaluation<T>[] NextDelayedAndGateEvaluations()
    {
        var nextDelayedAndGateEvaluations = _delayedAndGateEvaluations.ToArray();
        _delayedAndGateEvaluations.Clear();
        return nextDelayedAndGateEvaluations;
    }

    private class OptionalOutputValue
    {
        private T _value = default!;
        private bool _isPresent;

        public T Value
        {
            get => _isPresent ? _value : throw new InvalidOperationException("No output value is present for the given gate.");

            set
            {
                if (_isPresent)
                    throw new InvalidOperationException("Another output value is already present for the given gate.");

                _isPresent = true;
                _value = value;
            }
        }
    }
}
