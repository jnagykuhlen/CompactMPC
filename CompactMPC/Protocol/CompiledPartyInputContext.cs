using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Collections;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public class CompiledPartyInputContext(IReadOnlyList<InputExpressionDescription> expressionDescriptions)
{
    public IReadOnlyList<InputExpressionDescription> ExpressionDescriptions { get; } = expressionDescriptions;

    public int TotalNumberOfBits { get; } =
        expressionDescriptions.Sum(description => description.Expression.Wires.Count);

    public IEnumerable<Wire> Wires => ExpressionDescriptions.SelectMany(description => description.Expression.Wires);

    public BitArray GetBits(SecureProgramInput programInput)
    {
        var bits = new BitArray(TotalNumberOfBits);
        var bitsWriter = bits.GetWriter();

        foreach (var description in ExpressionDescriptions)
            description.GetInputValue(programInput).WriteTo(bitsWriter.NextSlice(description.Expression.Wires.Count));

        return bits;
    }
}
