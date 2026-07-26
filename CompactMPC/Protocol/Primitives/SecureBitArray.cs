using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Collections;

namespace CompactMPC.Protocol.Primitives;

public class SecureBitArray(IReadOnlyList<Wire> wires) : Expression(wires),
    IInputExpression<IReadOnlyList<Bit>>, IOutputExpression<BitArray>
{
    public void WriteTo(IReadOnlyList<Bit> value, IWriteOnlyList<Bit> destination)
    {
        for (var i = 0; i < value.Count; ++i)
            destination[i] = value[i];
    }

    public BitArray ReadFrom(IReadOnlyList<Bit> source) => new(source);

    public static SecureBitArray AllZeroes(int numberOfBits) =>
        new(Enumerable.Repeat(Wire.Zero, numberOfBits).ToArray());

    public static SecureBitArray AllOnes(int numberOfBits) =>
        new(Enumerable.Repeat(Wire.One, numberOfBits).ToArray());

    public static SecureBitArray Xor(IReadOnlyList<SecureBitArray> values) =>
        values.AggregateDepthEfficient((x, y) => x ^ y);

    public static SecureBitArray And(IReadOnlyList<SecureBitArray> values) =>
        values.AggregateDepthEfficient((x, y) => x & y);

    public static SecureBitArray operator ^(SecureBitArray left, SecureBitArray right) =>
        new(left.Wires.Zip(right.Wires, Wire.Xor).ToArray());

    public static SecureBitArray operator &(SecureBitArray left, SecureBitArray right) =>
        new(left.Wires.Zip(right.Wires, Wire.And).ToArray());

    public static SecureBitArray operator ~(SecureBitArray left) =>
        new(left.Wires.Select(Wire.Not).ToArray());

    public static Input<SecureBitArray> Input(int numberOfBits) => new(() => Assignable(numberOfBits));
    public static Output<SecureBitArray> Output() => new();

    public static SecureBitArray Assignable(int numberOfBits) =>
        new(
            Enumerable
                .Range(0, numberOfBits)
                .Select(_ => Wire.Assignable())
                .ToArray()
        );
}
