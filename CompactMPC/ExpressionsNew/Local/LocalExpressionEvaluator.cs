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
        public T Evaluate<T>(IOutputExpression<T> expression, params ExpressionInputBinding[] inputBindings)
        {
            IEnumerable<WireValue<Bit>> wireInputs = inputBindings.SelectMany(
                inputBinding => inputBinding.WireValues
            );
            
            IEnumerable<Wire> outputWires = expression.Wires
                .Where(wire => !wire.IsConstant);
            
            IReadOnlyDictionary<Wire, Bit> gateOutputs = ForwardCircuitEvaluation.From(LocalCircuitEvaluator.Instance)
                .Input(wireInputs)
                .Output(outputWires)
                .Execute();
            
            IReadOnlyList<Bit> outputBits = expression.Wires
                .Select(wire => wire.ConstantValue ?? gateOutputs[wire])
                .ToList();

            return expression.FromBits(outputBits);
        }
    }
}