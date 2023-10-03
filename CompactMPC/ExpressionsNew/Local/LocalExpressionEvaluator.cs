using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;

namespace CompactMPC.ExpressionsNew.Local
{
    public class LocalExpressionEvaluator
    {
        public T Evaluate<T>(IOutputExpression<T> expression, params ExpressionInputBinding[] inputBindings)
        {
            IReadOnlySet<ForwardGate> outputGates = expression.Wires
                .Where(wire => !wire.IsConstant)
                .Select(wire => wire.Gate)
                .ToHashSet();

            IReadOnlyDictionary<ForwardGate, Bit> gateInputs = new Dictionary<ForwardGate, Bit>(
                inputBindings.SelectMany(inputBinding => inputBinding.Wires
                    .Select(wire => wire.Gate)
                    .Zip(inputBinding.Bits, KeyValuePair.Create))
            );

            IReadOnlyDictionary<ForwardGate, Bit> gateOutputs =
                ForwardCircuit.Evaluate(LocalCircuitEvaluator.Instance, gateInputs, outputGates);

            IReadOnlyList<Bit> outputBits = expression.Wires
                .Select(wire => wire.ConstantValue ?? gateOutputs[wire.Gate])
                .ToList();

            return expression.FromBits(outputBits);
        }
    }
}