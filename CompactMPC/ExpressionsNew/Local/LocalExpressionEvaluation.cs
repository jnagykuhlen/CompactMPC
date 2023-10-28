using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Circuits.New;

namespace CompactMPC.ExpressionsNew.Local
{
    public class LocalExpressionEvaluation
    {
        private readonly ForwardCircuitEvaluation<Bit> _circuitEvaluation =
            ForwardCircuitEvaluation.From(LocalCircuitEvaluator.Instance);
        
        public LocalExpressionEvaluation Input(ExpressionValue expressionValue)
        {
            _circuitEvaluation.Input(expressionValue.WireValues);
            return this;
        }

        public LocalExpressionEvaluation Input<T>(IInputExpression<T> expression, T value)
        {
            return Input(ExpressionValue.From(expression, value));
        }

        public LocalExpressionEvaluation Output<T>(IOutputExpression<T> expression)
        {
            _circuitEvaluation.Output(expression.Wires.Where(wire => !wire.IsConstant));
            return this;
        }

        public LocalExpressionEvaluationResult Evaluate()
        {
            return new LocalExpressionEvaluationResult(_circuitEvaluation.Execute().ToDictionary());
        }
        
        public T Evaluate<T>(IOutputExpression<T> expression)
        {
            return Output(expression).Evaluate().Value(expression);
        }
    }
}