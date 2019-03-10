using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits
{
    public class AsyncCircuitEvaluator<T> : ICircuitEvaluator<Task<T>>
    {
        private ICircuitEvaluator<T> _innerEvaluator;
        
        public AsyncCircuitEvaluator(ICircuitEvaluator<T> innerEvaluator)
        {
            _innerEvaluator = innerEvaluator;
        }
        
        public Task<T> EvaluateAndGate(Task<T> leftValue, Task<T> rightValue, GateContext gateContext, CircuitContext circuitContext)
        {
            return Task.WhenAll(leftValue, rightValue).ContinueWith(
                task => _innerEvaluator.EvaluateAndGate(task.Result[0], task.Result[1], gateContext, circuitContext)
            );
        }

        public Task<T> EvaluateXorGate(Task<T> leftValue, Task<T> rightValue, GateContext gateContext, CircuitContext circuitContext)
        {
            return Task.WhenAll(leftValue, rightValue).ContinueWith(
                task => _innerEvaluator.EvaluateXorGate(task.Result[0], task.Result[1], gateContext, circuitContext)
            );
        }

        public Task<T> EvaluateNotGate(Task<T> value, GateContext gateContext, CircuitContext circuitContext)
        {
            return value.ContinueWith(task => _innerEvaluator.EvaluateNotGate(task.Result, gateContext, circuitContext));
        }
    }
}
