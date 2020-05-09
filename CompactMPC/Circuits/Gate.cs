using System.Collections.Generic;

namespace CompactMPC.Circuits
{
    public abstract class Gate
    {
        public int Id { get; }

        protected Gate(int id)
        {
            Id = id;
        }

        public abstract void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState
        );

        public abstract IEnumerable<Gate> InputGates { get; }
    }
}
