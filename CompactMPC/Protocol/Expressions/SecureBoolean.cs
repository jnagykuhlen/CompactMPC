using System.Collections.Generic;
using CompactMPC.Circuits;
using CompactMPC.Collections;

namespace CompactMPC.Protocol.Expressions;

public class SecureBoolean(Wire wire) : Expression([wire]), IInputExpression<bool>, IOutputExpression<bool>
{
    public static readonly SecureBoolean False = new(Wire.Zero);
    public static readonly SecureBoolean True = new(Wire.One);

    public void WriteTo(bool value, IWriteOnlyList<Bit> destination) => destination[0] = new Bit(value);
    public bool ReadFrom(IReadOnlyList<Bit> source) => source[0].IsSet;

    public static SecureBoolean operator &(SecureBoolean left, SecureBoolean right) =>
        new(Wire.And(left.Wire, right.Wire));

    public static SecureBoolean operator ^(SecureBoolean left, SecureBoolean right) =>
        new(Wire.Xor(left.Wire, right.Wire));

    public static SecureBoolean operator |(SecureBoolean left, SecureBoolean right) =>
        new(Wire.Or(left.Wire, right.Wire));

    public static SecureBoolean operator !(SecureBoolean right) =>
        new(Wire.Not(right.Wire));

    public static bool operator false(SecureBoolean right) => right.Wire == Wire.Zero;
    public static bool operator true(SecureBoolean right) => right.Wire == Wire.One;

    public static Input<SecureBoolean> Input() => new(Assignable);
    public static Output<SecureBoolean> Output() => new();

    public static SecureBoolean Assignable() => new(Wire.Assignable());

    public Wire Wire => Wires[0];
}
