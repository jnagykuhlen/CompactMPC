using System;

namespace CompactMPC.Circuits
{
    public class EvaluationState<T>
    {
        private readonly IdMapping<Optional<T>> _gateEvaluationValues;

        public T[] Input { get; }
        public T[] Output { get; }
        
        public EvaluationState(T[] input, CircuitContext context)
        {
            _gateEvaluationValues = new IdMapping<Optional<T>>(Optional<T>.Empty, context.NumberOfGates);
            Input = input;
            Output = new T[context.NumberOfOutputGates];
        }

        public T GetInput(int index)
        {
            return Input[index];
        }

        public void SetOutput(int index, T value)
        {
            Output[index] = value;
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
    }
}
