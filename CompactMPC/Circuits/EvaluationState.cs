using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class EvaluationState<T>
    {
        private T[] _input;
        private T[] _output;
        private IdMapping<Optional<T>> _gateEvaluationValues;

        public EvaluationState(T[] input, CircuitContext context)
        {
            _input = input;
            _output = new T[context.NumberOfOutputGates];
            _gateEvaluationValues = new IdMapping<Optional<T>>(Optional<T>.Empty, context.NumberOfGates);
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
            Optional<T> gateEvaluationValue = _gateEvaluationValues[gate.Id];
            if (!gateEvaluationValue.IsPresent)
                throw new InvalidOperationException(String.Format("Gate with id {0} was not evaluated yet.", gate.Id));

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

        public T[] Output
        {
            get
            {
                return _output;
            }
        }
    }
}
