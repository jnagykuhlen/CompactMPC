using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class CircuitEvaluationState<TIn, TProcess, TOut>
    {
        private struct GateEvaluationState
        {
            public TProcess Value;
            public bool IsEvaluated;

            public GateEvaluationState(TProcess value, bool isEvaluated)
            {
                Value = value;
                IsEvaluated = isEvaluated;
            }
        }

        private TIn[] _input;
        private TOut[] _output;
        private IdMapping<GateEvaluationState> _gateEvaluationStates;

        public CircuitEvaluationState(TIn[] input, CircuitContext context)
        {
            _input = input;
            _output = new TOut[context.NumberOfOutputGates];
            _gateEvaluationStates = new IdMapping<GateEvaluationState>(new GateEvaluationState(default(TProcess), false), context.NumberOfGates);
        }

        public TIn GetInput(int index)
        {
            return _input[index];
        }

        public void SetOutput(int index, TOut value)
        {
            _output[index] = value;
        }

        public TProcess GetGateEvaluationValue(Gate gate)
        {
            GateEvaluationState gateEvaluationState = _gateEvaluationStates[gate.Context.CircuitUniqueId];
            if (!gateEvaluationState.IsEvaluated)
                throw new InvalidOperationException(String.Format("Gate with id {0} was not evaluated yet.", gate.Context.CircuitUniqueId));

            return gateEvaluationState.Value;
        }

        public void SetGateEvaluationValue(Gate gate, TProcess value)
        {
            _gateEvaluationStates[gate.Context.CircuitUniqueId] = new GateEvaluationState(value, true);
        }

        public bool IsGateEvaluated(Gate gate)
        {
            return _gateEvaluationStates[gate.Context.CircuitUniqueId].IsEvaluated;
        }

        public TOut[] Output
        {
            get
            {
                return _output;
            }
        }
    }
}
