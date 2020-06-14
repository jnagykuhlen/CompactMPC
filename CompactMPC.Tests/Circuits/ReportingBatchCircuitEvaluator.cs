﻿using System.Collections.Generic;
using CompactMPC.Circuits.Batching;

namespace CompactMPC.Circuits
{
    public class ReportingBatchCircuitEvaluator<T> : IBatchCircuitEvaluator<T>
    {
        private readonly IBatchCircuitEvaluator<T> _innerEvaluator;
        private readonly List<int> _batchSizes;

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
