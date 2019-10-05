using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;

namespace CompactMPC.Expressions
{
    public class SecureInteger : SecureWord
    {
        public static readonly SecureInteger Zero = FromConstant(0L);
        public static readonly SecureInteger One = FromConstant(1L);

        public SecureInteger(IReadOnlyList<Wire> wires)
            : base(wires) { }
        
        public static SecureInteger Sum(IEnumerable<SecureInteger> values)
        {
            return values.AggregateDepthEfficient((x, y) => x + y);
        }

        public static SecureInteger Sum(params SecureInteger[] values)
        {
            return Sum((IEnumerable<SecureInteger>)values);
        }

        public static SecureInteger FromWord(SecureWord word)
        {
            return new SecureInteger(word.Wires);
        }

        public static SecureInteger FromBoolean(SecureBoolean boolean)
        {
            return new SecureInteger(new[] { boolean.Wire });
        }
        
        public static SecureInteger FromConstant(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Constant value must not be negative.");

            if (value == 0)
                return new SecureInteger(Array.Empty<Wire>());

            Wire[] wires = new Wire[GetNumberOfRequiredBits(value)];
            for (int i = 0; i < wires.Length; ++i)
                wires[i] = (value & (1 << i)) != 0 ? Wire.One : Wire.Zero;

            return new SecureInteger(wires);
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
                return new SecureInteger(Wires.Take(length).ToArray());

            return new SecureInteger(Enumerable.Concat(Wires, Enumerable.Repeat(Wire.Zero, length - Wires.Count)).ToArray());
        }

        public static SecureInteger operator +(SecureInteger left, SecureInteger right)
        {
            int maxLength = Math.Max(left.Length, right.Length);

            Wire[] result = new Wire[maxLength + 1];
            Wire carryover = Wire.Zero;

            for (int i = 0; i < maxLength; ++i)
            {
                Wire leftWire = i < left.Length ? left.Wires[i] : Wire.Zero;
                Wire rightWire = i < right.Length ? right.Wires[i] : Wire.Zero;

                result[i] = Wire.Xor(Wire.Xor(leftWire, rightWire), carryover);
                carryover = Wire.Xor(
                    carryover,
                    Wire.And(
                        Wire.Xor(carryover, leftWire),
                        Wire.Xor(carryover, rightWire)
                    )
                );
            }

            result[maxLength] = carryover;

            return new SecureInteger(result);
        }

        public static SecureBoolean operator >(SecureInteger left, SecureInteger right)
        {
            int maxLength = Math.Max(left.Length, right.Length);

            Wire result = Wire.Zero;
            for (int i = 0; i < maxLength; ++i)
            {
                Wire leftWire = i < left.Length ? left.Wires[i] : Wire.Zero;
                Wire rightWire = i < right.Length ? right.Wires[i] : Wire.Zero;

                result = Wire.Xor(
                    leftWire,
                    Wire.And(
                        Wire.Xor(leftWire, result),
                        Wire.Xor(rightWire, result)
                    )
                );
            }

            return new SecureBoolean(result);
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
