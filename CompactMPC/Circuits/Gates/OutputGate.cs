using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Gates
{
    public class OutputGate : Gate
    {
        private Gate _inputGate;

        public OutputGate(GateContext context, Gate inputGate)
             : base(context)
        {
            _inputGate = inputGate;
        }

        public override void Evaluate<TIn, TProcess, TOut>(
            ICircuitEvaluator<TIn, TProcess, TOut> evaluator,
            CircuitEvaluationState<TIn, TProcess, TOut> evaluationState,
            CircuitContext circuitContext)
        {
            TOut outputValue = evaluator.EvaluateOutputGate(
                evaluationState.GetGateEvaluationValue(_inputGate),
                Context,
                circuitContext
            );

            evaluationState.SetOutput(Context.TypeUniqueId, outputValue);
        }

        public override IEnumerable<Gate> InputGates
        {
            get
            {
                yield return _inputGate;
            }
        }

        public Gate Input
        {
            get
            {
                return _inputGate;
            }
        }
    }
}
