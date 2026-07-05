using System;
using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits.New;
using CompactMPC.Expressions;
using CompactMPC.ExpressionsNew.Internal;
using CompactMPC.Protocol.New;

namespace CompactMPC.ExpressionsNew
{
    public class IntegerExpression(IReadOnlyList<Wire> wires, int maxValue) : Expression(wires),
        IInputExpression<int>, IOutputExpression<int>
    {
        public static readonly IntegerExpression Zero = new([], 0);
        public static readonly IntegerExpression One = new([Wire.One], 1);

        public int MaxValue { get; } = maxValue;

        public IReadOnlyList<Bit> ToBits(int value) =>
            IntegerBitConverter.Instance.ToBits(value, Wires.Count);

        public int FromBits(IReadOnlyList<Bit> bits) => IntegerBitConverter.Instance.FromBits(bits);

        public static IntegerExpression Sum(params IntegerExpression[] values) =>
            values.AggregateDepthEfficient((x, y) => x + y);

        public static IntegerExpression FromBoolean(BooleanExpression expression) => new(expression.Wires, 1);

        public static IntegerExpression Constant(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Constant value must not be negative.");

            if (value == 0)
                return Zero;

            if (value == 1)
                return One;

            var wires = new Wire[RequiredNumberOfBits(value)];
            for (var i = 0; i < wires.Length; ++i)
                wires[i] = (value & (1 << i)) != 0 ? Wire.One : Wire.Zero;

            return new IntegerExpression(wires, value);
        }

        private static int RequiredNumberOfBits(int maxValue)
        {
            int numberOfBits = 0;
            while (1 << numberOfBits <= maxValue)
                numberOfBits++;

            return numberOfBits;
        }

        public static IntegerExpression operator +(IntegerExpression left, IntegerExpression right)
        {
            var maxValue = left.MaxValue + right.MaxValue;
            var numberOfBits = RequiredNumberOfBits(maxValue);

            var result = new Wire[numberOfBits];
            var carryover = Wire.Zero;

            for (var i = 0; i < numberOfBits; ++i)
            {
                var leftWire = i < left.Wires.Count ? left.Wires[i] : Wire.Zero;
                var rightWire = i < right.Wires.Count ? right.Wires[i] : Wire.Zero;

                result[i] = Wire.Xor(Wire.Xor(leftWire, rightWire), carryover);

                if (i < numberOfBits - 1)
                {
                    carryover = Wire.Xor(
                        carryover,
                        Wire.And(
                            Wire.Xor(carryover, leftWire),
                            Wire.Xor(carryover, rightWire)
                        )
                    );
                }
            }

            return new IntegerExpression(result, maxValue);
        }

        public static BooleanExpression operator >(IntegerExpression left, IntegerExpression right)
        {
            var maxLength = Math.Max(left.Wires.Count, right.Wires.Count);

            var result = Wire.Zero;
            for (var i = 0; i < maxLength; ++i)
            {
                var leftWire = i < left.Wires.Count ? left.Wires[i] : Wire.Zero;
                var rightWire = i < right.Wires.Count ? right.Wires[i] : Wire.Zero;

                result = Wire.Xor(
                    leftWire,
                    Wire.And(
                        Wire.Xor(leftWire, result),
                        Wire.Xor(rightWire, result)
                    )
                );
            }

            return new BooleanExpression(result);
        }

        public static BooleanExpression operator <(IntegerExpression left, IntegerExpression right) => right > left;
        public static BooleanExpression operator >=(IntegerExpression left, IntegerExpression right) => !(right > left);
        public static BooleanExpression operator <=(IntegerExpression left, IntegerExpression right) => !(left > right);

        public static Input<IntegerExpression> Input(int maxValue) => new(() => AssignableUpTo(maxValue));
        public static Output<IntegerExpression> Output() => new();

        public static IntegerExpression AssignableUpTo(int maxValue)
        {
            var wires = Enumerable
                .Range(0, RequiredNumberOfBits(maxValue))
                .Select(_ => Wire.Assignable())
                .ToArray();

            return new IntegerExpression(wires, maxValue);
        }
    }
}
