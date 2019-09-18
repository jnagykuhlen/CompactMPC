using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Batching;

namespace CompactMPC.UnitTests.Circuits
{
    public class ReportingBatchCircuitEvaluator<T> : IBatchCircuitEvaluator<T>
    {
        private IBatchCircuitEvaluator<T> _innerEvaluator;
        private List<int> _batchSizes;

        public ReportingBatchCircuitEvaluator(IBatchCircuitEvaluator<T> innerEvaluator)
        {
            _innerEvaluator = innerEvaluator;
            _batchSizes = new List<int>();
        }

        public T[] EvaluateAndGateBatch(GateEvaluationInput<T>[] evaluationInputs)
        {
            _batchSizes.Add(evaluationInputs.Length);
            return _innerEvaluator.EvaluateAndGateBatch(evaluationInputs);
        }

        public T EvaluateXorGate(T leftValue, T rightValue)
        {
            return _innerEvaluator.EvaluateXorGate(leftValue, rightValue);
        }

        public T EvaluateNotGate(T value)
        {
            return _innerEvaluator.EvaluateNotGate(value);
        }

        public int[] BatchSizes
        {
            get
            {
                return _batchSizes.ToArray();
            }
        }
    }
}
