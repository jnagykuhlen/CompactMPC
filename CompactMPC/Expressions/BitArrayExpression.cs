using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Collections;
using CompactMPC.Protocol;

namespace CompactMPC.Expressions;

public class BitArrayExpression(IReadOnlyList<Wire> wires) : Expression(wires),
    IInputExpression<IReadOnlyList<Bit>>, IOutputExpression<BitArray>
{
    public void WriteTo(IReadOnlyList<Bit> value, IWriteOnlyList<Bit> destination)
    {
        for (var i = 0; i < value.Count; ++i)
            destination[i] = value[i];
    }

    public BitArray ReadFrom(IReadOnlyList<Bit> source) => new(source);

    public static BitArrayExpression AllZeroes(int numberOfBits) =>
        new(Enumerable.Repeat(Wire.Zero, numberOfBits).ToArray());

    public static BitArrayExpression AllOnes(int numberOfBits) =>
        new(Enumerable.Repeat(Wire.One, numberOfBits).ToArray());

    public static BitArrayExpression Xor(IReadOnlyList<BitArrayExpression> values) =>
        values.AggregateDepthEfficient((x, y) => x ^ y);

    public static BitArrayExpression And(IReadOnlyList<BitArrayExpression> values) =>
        values.AggregateDepthEfficient((x, y) => x & y);

    public static BitArrayExpression operator ^(BitArrayExpression left, BitArrayExpression right) =>
        new(left.Wires.Zip(right.Wires, Wire.Xor).ToArray());

    public static BitArrayExpression operator &(BitArrayExpression left, BitArrayExpression right) =>
        new(left.Wires.Zip(right.Wires, Wire.And).ToArray());

    public static BitArrayExpression operator ~(BitArrayExpression left) =>
        new(left.Wires.Select(Wire.Not).ToArray());

    public static Input<BitArrayExpression> Input(int numberOfBits) => new(() => Assignable(numberOfBits));
    public static Output<BitArrayExpression> Output() => new();

    private static BitArrayExpression Assignable(int numberOfBits) =>
        new(
            Enumerable
                .Range(0, numberOfBits)
                .Select(_ => Wire.Assignable())
                .ToArray()
        );
}
