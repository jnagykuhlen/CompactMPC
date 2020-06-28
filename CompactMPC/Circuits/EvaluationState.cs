using System;
using System.Collections.Generic;

namespace CompactMPC.Circuits
{
    public class EvaluationState<T>
    {
        public IReadOnlyList<T> Input { get; }

        private readonly T[] _output;
        private readonly IdMapping<Optional<T>> _gateEvaluationValues;

        public EvaluationState(IReadOnlyList<T> input, CircuitContext context)
        {
            Input = input;
            _output = new T[context.NumberOfOutputWires];
            _gateEvaluationValues = new IdMapping<Optional<T>>(Optional<T>.Empty, context.NumberOfGates);
        }
        
        public void SetOutput(int index, T value)
        {
            _output[index] = value;
        }

        public T GetGateEvaluationValue(Gate gate)
        {
            Optional<T> gateEvaluationValue = _gateEvaluationValues[gate.Id];
            if (!gateEvaluationValue.IsPresent)
                throw new InvalidOperationException($"Gate with id {gate.Id} was not evaluated yet.");

            return gateEvaluationValue.Value;
        }

        public void SetGateEvaluationValue(Gate gate, T value)
        {
            _gateEvaluationValues[gate.Id] = Optional<T>.FromValue(value);
        }

        public bool IsGateEvaluated(Gate gate)
        {
            return _gateEvaluationValues[gate.Id].IsPresent;
        }

        public IReadOnlyList<T> Output
        {
            get
            {
                return _output;
            }
        }
    }
}
