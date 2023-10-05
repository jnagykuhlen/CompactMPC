using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Circuits.New;
using Wire = CompactMPC.Circuits.New.Wire;

namespace CompactMPC.ExpressionsNew.Local
{
    // TODO: Rename to LocalExpressionEvaluation and implement as builder
    public class LocalExpressionEvaluator
    {
        public T Evaluate<T>(IOutputExpression<T> outputExpression, params ExpressionValue[] inputExpressionValues)
        {
            IEnumerable<WireValue<Bit>> wireInputs = inputExpressionValues.SelectMany(
                inputExpressionValue => inputExpressionValue.WireValues
            );
            
            IEnumerable<Wire> outputWires = outputExpression.Wires
                .Where(wire => !wire.IsConstant);
            
            IReadOnlyDictionary<Wire, Bit> wireOutputs = ForwardCircuitEvaluation.From(LocalCircuitEvaluator.Instance)
                .Input(wireInputs)
                .Output(outputWires)
                .Execute();
            
            IReadOnlyList<Bit> outputBits = outputExpression.Wires
                .Select(wire => wire.ConstantValue ?? wireOutputs[wire])
                .ToList();

            return outputExpression.FromBits(outputBits);
        }
    }
}