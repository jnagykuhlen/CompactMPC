using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class BatchedCircuitEvaluator<T> : IBatchedCircuitEvaluator<T>
    {
        private ICircuitEvaluator<T> _innerEvaluator;

        public BatchedCircuitEvaluator(ICircuitEvaluator<T> innerEvaluator)
        {
            _innerEvaluator = innerEvaluator;
        }
        
        public T[] EvaluateAndGateBatch(GateEvaluationInput<T>[] evaluationInputs, CircuitContext circuitContext)
        {
            T[] outputValues = new T[evaluationInputs.Length];
            for (int i = 0; i < evaluationInputs.Length; ++i)
            {
                GateEvaluationInput<T> evaluationInput = evaluationInputs[i];
                outputValues[i] = _innerEvaluator.EvaluateAndGate(
                    evaluationInput.LeftValue,
                    evaluationInput.RightValue,
                    evaluationInput.Context,
                    circuitContext
                );
            }

            return outputValues;
        }

        public T EvaluateXorGate(T leftValue, T rightValue, GateContext gateContext, CircuitContext circuitContext)
        {
            return _innerEvaluator.EvaluateXorGate(leftValue, rightValue, gateContext, circuitContext);
        }

        public T EvaluateNotGate(T value, GateContext gateContext, CircuitContext circuitContext)
        {
            return _innerEvaluator.EvaluateNotGate(value, gateContext, circuitContext);
        }
    }
}
