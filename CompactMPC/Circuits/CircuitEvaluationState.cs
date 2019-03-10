using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class CircuitEvaluationState<T>
    {
        private struct GateEvaluationState
        {
            public T Value;
            public bool IsEvaluated;

            public GateEvaluationState(T value, bool isEvaluated)
            {
                Value = value;
                IsEvaluated = isEvaluated;
            }
        }

        private T[] _input;
        private T[] _output;
        private IdMapping<GateEvaluationState> _gateEvaluationStates;

        public CircuitEvaluationState(T[] input, CircuitContext context)
        {
            _input = input;
            _output = new T[context.NumberOfOutputGates];
            _gateEvaluationStates = new IdMapping<GateEvaluationState>(new GateEvaluationState(default(T), false), context.NumberOfGates);
        }

        public T GetInput(int index)
        {
            return _input[index];
        }

        public void SetOutput(int index, T value)
        {
            _output[index] = value;
        }

        public T GetGateEvaluationValue(Gate gate)
        {
            GateEvaluationState gateEvaluationState = _gateEvaluationStates[gate.Context.CircuitUniqueId];
            if (!gateEvaluationState.IsEvaluated)
                throw new InvalidOperationException(String.Format("Gate with id {0} was not evaluated yet.", gate.Context.CircuitUniqueId));

            return gateEvaluationState.Value;
        }

        public void SetGateEvaluationValue(Gate gate, T value)
        {
            _gateEvaluationStates[gate.Context.CircuitUniqueId] = new GateEvaluationState(value, true);
        }

        public bool IsGateEvaluated(Gate gate)
        {
            return _gateEvaluationStates[gate.Context.CircuitUniqueId].IsEvaluated;
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
