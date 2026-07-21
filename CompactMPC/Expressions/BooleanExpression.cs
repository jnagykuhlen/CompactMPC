using System.Collections.Generic;
using CompactMPC.Circuits;
using CompactMPC.Collections;

namespace CompactMPC.Expressions;

public class BooleanExpression(Wire wire) : Expression([wire]), IInputExpression<bool>, IOutputExpression<bool>
{
    public static readonly BooleanExpression False = new(Wire.Zero);
    public static readonly BooleanExpression True = new(Wire.One);

    public void WriteTo(bool value, IWriteOnlyList<Bit> destination) => destination[0] = new Bit(value);
    public bool ReadFrom(IReadOnlyList<Bit> source) => source[0].IsSet;

    public static BooleanExpression Assignable() => new(Wire.Assignable());

    public static BooleanExpression operator &(BooleanExpression left, BooleanExpression right) =>
        new(Wire.And(left.Wire, right.Wire));

    public static BooleanExpression operator ^(BooleanExpression left, BooleanExpression right) =>
        new(Wire.Xor(left.Wire, right.Wire));

    public static BooleanExpression operator |(BooleanExpression left, BooleanExpression right) =>
        new(Wire.Or(left.Wire, right.Wire));

    public static BooleanExpression operator !(BooleanExpression right) =>
        new(Wire.Not(right.Wire));

    public static bool operator false(BooleanExpression right) => right.Wire == Wire.Zero;
    public static bool operator true(BooleanExpression right) => right.Wire == Wire.One;

    public Wire Wire => Wires[0];
}
