using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Collections;

namespace CompactMPC.Protocol.Expressions;

public class SecureBitArray(IReadOnlyList<Wire> wires) : Expression(wires),
    IInputExpression<IReadOnlyList<Bit>>, IOutputExpression<BitArray>, IMultiplexable<SecureBitArray>
{
    public SecureBitArray(Wire wire, int numberOfBits) :
        this(Enumerable.Repeat(wire, numberOfBits).ToArray()) { }

    public void WriteTo(IReadOnlyList<Bit> value, IWriteOnlyList<Bit> destination)
    {
        for (var i = 0; i < value.Count; ++i)
            destination[i] = value[i];
    }

    public BitArray ReadFrom(IReadOnlyList<Bit> source) => new(source);

    public SecureBitArray Xor(SecureBitArray other) =>
        new(Wires.Zip(other.Wires, Wire.Xor).ToArray());

    public SecureBitArray And(SecureBitArray other) =>
        new(Wires.Zip(other.Wires, Wire.And).ToArray());

    public SecureBitArray Not() =>
        new(Wires.Select(Wire.Not).ToArray());

    public SecureBoolean IsBitSet(int index) => new(Wires[index]);

    public static SecureBitArray AllZeroes(int numberOfBits) =>
        new(Wire.Zero, numberOfBits);

    public static SecureBitArray AllOnes(int numberOfBits) =>
        new(Wire.One, numberOfBits);

    public static SecureBitArray Xor(IReadOnlyList<SecureBitArray> values) =>
        values.AggregateDepthEfficient((x, y) => x.Xor(y));

    public static SecureBitArray And(IReadOnlyList<SecureBitArray> values) =>
        values.AggregateDepthEfficient((x, y) => x.And(y));

    public static SecureBitArray Multiplex(SecureBoolean condition, SecureBitArray ifTrue, SecureBitArray ifFalse) =>
        new(IMultiplexable<SecureBitArray>.Multiplex(condition.Wire, ifTrue.Wires, ifFalse.Wires));

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
