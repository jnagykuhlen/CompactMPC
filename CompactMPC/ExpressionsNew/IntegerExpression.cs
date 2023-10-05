using System;
using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits.New;
using CompactMPC.Expressions;
using CompactMPC.ExpressionsNew.Internal;

namespace CompactMPC.ExpressionsNew
{
    public class IntegerExpression : Expression, IInputExpression<int>, IOutputExpression<int>
    {
        public static readonly IntegerExpression Zero = new IntegerExpression(new Wire[] { }, 0);
        public static readonly IntegerExpression One = new IntegerExpression(new[] { Wire.One }, 1);

        public int MaxValue { get; }

        public IntegerExpression(IReadOnlyList<Wire> wires, int maxValue)
            : base(wires)
        {
            MaxValue = maxValue;
        }

        public IReadOnlyList<Bit> ToBits(int value)
        {
            return IntegerBitConverter.Instance.ToBits(value, Wires.Count);
        }

        public int FromBits(IReadOnlyList<Bit> bits)
        {
            return IntegerBitConverter.Instance.FromBits(bits);
        }

        public static IntegerExpression Sum(params IntegerExpression[] values)
        {
            return values.AggregateDepthEfficient((x, y) => x + y);
        }

        public static IntegerExpression FromBoolean(BooleanExpression expression)
        {
            return new IntegerExpression(expression.Wires, 1);
        }

        public static IntegerExpression FromConstant(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Constant value must not be negative.");

            if (value == 0)
                return Zero;

            if (value == 1)
                return One;

            Wire[] wires = new Wire[RequiredNumberOfBits(value)];
            for (int i = 0; i < wires.Length; ++i)
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
            int maxValue = left.MaxValue + right.MaxValue;
            int numberOfBits = RequiredNumberOfBits(maxValue);

            Wire[] result = new Wire[numberOfBits];
            Wire carryover = Wire.Zero;

            for (int i = 0; i < numberOfBits; ++i)
            {
                Wire leftWire = i < left.Wires.Count ? left.Wires[i] : Wire.Zero;
                Wire rightWire = i < right.Wires.Count ? right.Wires[i] : Wire.Zero;

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
            int maxLength = Math.Max(left.Wires.Count, right.Wires.Count);

            Wire result = Wire.Zero;
            for (int i = 0; i < maxLength; ++i)
            {
                Wire leftWire = i < left.Wires.Count ? left.Wires[i] : Wire.Zero;
                Wire rightWire = i < right.Wires.Count ? right.Wires[i] : Wire.Zero;

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

        public static BooleanExpression operator <(IntegerExpression left, IntegerExpression right)
        {
            return right > left;
        }

        public static BooleanExpression operator >=(IntegerExpression left, IntegerExpression right)
        {
            return !(right > left);
        }

        public static BooleanExpression operator <=(IntegerExpression left, IntegerExpression right)
        {
            return !(left > right);
        }

        public static IntegerExpression FromInput(int maxValue)
        {
            Wire[] wires = Enumerable
                .Range(0, RequiredNumberOfBits(maxValue))
                .Select(i => Wire.Input())
                .ToArray();

            return new IntegerExpression(wires, maxValue);
        }
    }
}
