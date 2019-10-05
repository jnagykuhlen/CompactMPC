using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;

namespace CompactMPC.Expressions
{
    public class SecureWord : SecurePrimitive
    {
        private IReadOnlyList<Wire> _wires;

        public SecureWord(IReadOnlyList<Wire> wires)
        {
            _wires = wires;
        }

        public static SecureWord FromConstant(BitArray bits)
        {
            return new SecureWord(bits.Select(bit => bit.IsSet ? Wire.One : Wire.Zero).ToArray());
        }

        public SecureWord GetSegment(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex >= Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (startIndex + length > Length)
                throw new ArgumentException("Requested segment length exceeds buffer.", nameof(length));

            return new SecureWord(_wires.Skip(startIndex).Take(length).ToArray());
        }

        public SecureBoolean IsBitSet(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return new SecureBoolean(_wires[index]);
        }

        public SecureWord FilterFirst()
        {
            if (Length <= 1)
                return this;

            Wire[] result = new Wire[Length];
            Wire condition = Wire.One;

            result[0] = _wires[0];

            for (int i = 1; i < result.Length; ++i)
            {
                condition = Wire.And(condition, Wire.Not(_wires[i - 1]));
                result[i] = Wire.And(_wires[i], condition);
            }

            return new SecureWord(result);
        }

        public static SecureWord And(IEnumerable<SecureWord> values)
        {
            return values.AggregateDepthEfficient((x, y) => x & y);
        }

        public static SecureWord And(params SecureWord[] values)
        {
            return And((IEnumerable<SecureWord>)values);
        }

        public static SecureWord Or(IEnumerable<SecureWord> values)
        {
            return values.AggregateDepthEfficient((x, y) => x | y);
        }

        public static SecureWord Or(params SecureWord[] values)
        {
            return Or((IEnumerable<SecureWord>)values);
        }

        public static SecureWord Xor(IEnumerable<SecureWord> values)
        {
            return values.AggregateDepthEfficient((x, y) => x ^ y);
        }

        public static SecureWord Xor(params SecureWord[] values)
        {
            return Xor((IEnumerable<SecureWord>)values);
        }
        
        public static SecureWord Multiplex(SecureWord left, SecureWord right, SecureBoolean condition)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("Secure words must be of same length for multiplexing.");

            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < right.Length; ++i)
            {
                result[i] = Wire.Xor(
                    left.Wires[i],
                    Wire.And(
                        condition.Wire,
                        Wire.Xor(left.Wires[i], right.Wires[i])
                    )
                );
            }

            return new SecureWord(result);
        }

        public override bool Equals(object other)
        {
            return other is SecureWord && Enumerable.SequenceEqual(_wires, ((SecureWord)other).Wires);
        }

        public override int GetHashCode()
        {
            int hashCode = 1097976886;
            foreach (Wire wire in _wires)
                hashCode = hashCode * -1521134295 + wire.GetHashCode();

            return hashCode;
        }

        public static SecureWord operator &(SecureWord left, SecureWord right)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("Secure words must be of same length for bitwise logical conjunction.");
            
            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = Wire.And(left.Wires[i], right.Wires[i]);

            return new SecureWord(result);
        }

        public static SecureWord operator ^(SecureWord left, SecureWord right)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("Secure words must be of same length for bitwise logical XOR.");
            
            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = Wire.Xor(left.Wires[i], right.Wires[i]);

            return new SecureWord(result);
        }

        public static SecureWord operator |(SecureWord left, SecureWord right)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("Secure words must be of same length for bitwise logical disjunction.");
            
            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = Wire.Or(left.Wires[i], right.Wires[i]);

            return new SecureWord(result);
        }

        public static SecureWord operator ~(SecureWord right)
        {
            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = Wire.Not(right.Wires[i]);

            return new SecureWord(result);
        }

        public static SecureBoolean operator ==(SecureWord left, SecureWord right)
        {
            Wire result = Enumerable.Zip(left.Wires, right.Wires, (leftWire, rightWire) => Wire.Not(Wire.Xor(leftWire, rightWire)))
                .AggregateDepthEfficient((x, y) => Wire.And(x, y));

            return new SecureBoolean(result);
        }

        public static SecureBoolean operator !=(SecureWord left, SecureWord right)
        {
            return !(left == right);
        }

        public int Length
        {
            get
            {
                return Wires.Count;
            }
        }

        public override IReadOnlyList<Wire> Wires
        {
            get
            {
                return _wires;
            }
        }
    }
}
