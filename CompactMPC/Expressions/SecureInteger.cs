using System;
using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;

namespace CompactMPC.Expressions
{
    public class SecureInteger : SecureWord
    {
        public SecureInteger(CircuitBuilder builder, IEnumerable<Wire> wires)
            : base(builder, wires) { }

        public static SecureInteger Sum(params SecureInteger[] values)
        {
            return values.AggregateDepthEfficient((x, y) => x + y);
        }

        public static SecureInteger FromWord(SecureWord word)
        {
            return new SecureInteger(word.Builder, word.Wires);
        }

        public static SecureInteger FromBoolean(SecureBoolean boolean)
        {
            return new SecureInteger(boolean.Builder, boolean.Wires);
        }

        public static SecureInteger Zero(CircuitBuilder builder)
        {
            return FromConstant(builder, 0);
        }

        public static SecureInteger One(CircuitBuilder builder)
        {
            return FromConstant(builder, 1);
        }
        
        public static SecureInteger FromConstant(CircuitBuilder builder, long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Constant value must not be negative.");

            if (value == 0)
                return new SecureInteger(builder, Enumerable.Empty<Wire>());

            Wire[] wires = new Wire[GetNumberOfRequiredBits(value)];
            for (int i = 0; i < wires.Length; ++i)
                wires[i] = (value & (1 << i)) != 0 ? Wire.One : Wire.Zero;

            return new SecureInteger(builder, wires);
        }
        
        private static int GetNumberOfRequiredBits(long value)
        {
            int numberOfBits = 1;
            while ((value >>= 1) != 0)
                ++numberOfBits;

            return numberOfBits;
        }

        public SecureInteger OfFixedLength(int length)
        {
            if (Wires.Count >= length)
                return new SecureInteger(Builder, Wires.Take(length));

            return new SecureInteger(Builder, Wires.Concat(Enumerable.Repeat(Wire.Zero, length - Wires.Count)));
        }

        public static SecureInteger operator +(SecureInteger left, SecureInteger right)
        {
            if (left.Builder != right.Builder)
                throw new ArgumentException("Secure integers must use the same circuit builder for constructing gates.");

            int maxLength = Math.Max(left.Length, right.Length);

            Wire[] result = new Wire[maxLength + 1];
            Wire carryover = Wire.Zero;

            for (int i = 0; i < maxLength; ++i)
            {
                Wire leftWire = i < left.Length ? left.Wires[i] : Wire.Zero;
                Wire rightWire = i < right.Length ? right.Wires[i] : Wire.Zero;

                result[i] = right.Builder.Xor(right.Builder.Xor(leftWire, rightWire), carryover);
                carryover = right.Builder.Xor(
                    carryover,
                    right.Builder.And(
                        right.Builder.Xor(carryover, leftWire),
                        right.Builder.Xor(carryover, rightWire)
                    )
                );
            }

            result[maxLength] = carryover;

            return new SecureInteger(right.Builder, result);
        }

        public static SecureBoolean operator >(SecureInteger left, SecureInteger right)
        {
            if (left.Builder != right.Builder)
                throw new ArgumentException("Secure integers must use the same circuit builder for constructing gates.");

            int maxLength = Math.Max(left.Length, right.Length);

            Wire result = Wire.Zero;
            for (int i = 0; i < maxLength; ++i)
            {
                Wire leftWire = i < left.Length ? left.Wires[i] : Wire.Zero;
                Wire rightWire = i < right.Length ? right.Wires[i] : Wire.Zero;

                result = right.Builder.Xor(
                    leftWire,
                    right.Builder.And(
                        right.Builder.Xor(leftWire, result),
                        right.Builder.Xor(rightWire, result)
                    )
                );
            }

            return new SecureBoolean(right.Builder, result);
        }

        public static SecureBoolean operator <(SecureInteger left, SecureInteger right)
        {
            return right > left;
        }

        public static SecureBoolean operator >=(SecureInteger left, SecureInteger right)
        {
            return !(right > left);
        }

        public static SecureBoolean operator <=(SecureInteger left, SecureInteger right)
        {
            return !(left > right);
        }
    }
}
