using System;
using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;

namespace CompactMPC.Protocol.Expressions;

public interface IMultiplexable<T> : IExpression
{
    static abstract T Multiplex(SecureBoolean condition, T ifTrue, T ifFalse);

    protected static Wire Multiplex(Wire condition, Wire trueWire, Wire falseWire) =>
        Wire.Xor(falseWire, Wire.And(condition, Wire.Xor(falseWire, trueWire)));

    protected static IReadOnlyList<Wire> Multiplex(Wire condition, IReadOnlyList<Wire> trueWires, IReadOnlyList<Wire> falseWires)
    {
        if (trueWires.Count != falseWires.Count)
            throw new ArgumentException("Cannot multiplex different numbers of wires.");

        return trueWires.Zip(
            falseWires,
            (trueWire, falseWire) => Multiplex(condition, trueWire, falseWire)
        ).ToList();
    }
}
