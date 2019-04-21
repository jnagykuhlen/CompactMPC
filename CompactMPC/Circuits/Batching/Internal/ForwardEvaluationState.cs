using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardEvaluationState<T>
    {
        private T[] _input;
        private T[] _output;
        private Dictionary<ForwardGate, List<T>> _inputValuesByGate;
        private Queue<BatchElement<T>> _delayedAndGateEvaluations;

        public ForwardEvaluationState(T[] input, int numberOfOutputGates)
        {
            _input = input;
            _output = new T[numberOfOutputGates];
            _inputValuesByGate = new Dictionary<ForwardGate, List<T>>();
            _delayedAndGateEvaluations = new Queue<BatchElement<T>>();
        }
        
        public T GetInput(int index)
        {
            return _input[index];
        }

        public void SetOutput(int index, T value)
        {
            _output[index] = value;
        }

        public void PushInputValue(ForwardGate gate, T value)
        {
            List<T> inputValues;
            if (!_inputValuesByGate.TryGetValue(gate, out inputValues))
            {
                inputValues = new List<T>(2);
                _inputValuesByGate.Add(gate, inputValues);
            }

            inputValues.Add(value);
        }

        public IReadOnlyList<T> PullInputValues(ForwardGate gate)
        {
            List<T> inputValues;
            if (_inputValuesByGate.TryGetValue(gate, out inputValues))
            {
                _inputValuesByGate.Remove(gate);
                return inputValues;
            }

            return Array.Empty<T>();
        }

        public int GetNumberOfInputValues(ForwardGate gate)
        {
            List<T> inputValues;
            if (_inputValuesByGate.TryGetValue(gate, out inputValues))
                return inputValues.Count;

            return 0;
        }

        public void DelayAndGateEvaluation(BatchElement<T> batchElement)
        {
            _delayedAndGateEvaluations.Enqueue(batchElement);
        }

        public BatchElement<T>[] GetNextAndGateBatch()
        {
            BatchElement<T>[] nextAndGateBatch = _delayedAndGateEvaluations.ToArray();
            _delayedAndGateEvaluations.Clear();
            return nextAndGateBatch;
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
