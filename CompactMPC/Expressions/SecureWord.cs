﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CompactMPC.Circuits;

namespace CompactMPC.Expressions
{
    public class SecureWord : SecurePrimitive
    {
        private SecureWord(CircuitBuilder builder, Wire[] wires)
            : base(builder, wires) { }

        public SecureWord(CircuitBuilder builder, IEnumerable<Wire> wires)
            : base(builder, wires.ToArray()) { }

        public static SecureWord FromConstant(CircuitBuilder builder, BitArray bits)
        {
            return new SecureWord(builder, bits.Select(bit => bit.IsSet ? Wire.One : Wire.Zero));
        }

        public override bool Equals(object? other)
        {
            return ReferenceEquals(this, other);
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        public SecureWord GetSegment(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex >= Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (startIndex + length > Length)
                throw new ArgumentException("Requested segment length exceeds buffer.", nameof(length));

            return new SecureWord(Builder, Wires.Skip(startIndex).Take(length));
        }

        public SecureBoolean IsBitSet(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return new SecureBoolean(Builder, Wires[index]);
        }

        public SecureWord FilterFirst()
        {
            if (Length <= 1)
                return this;

            Wire[] result = new Wire[Length];
            Wire condition = Wire.One;

            result[0] = Wires[0];

            for (int i = 1; i < result.Length; ++i)
            {
                condition = Builder.And(condition, Builder.Not(Wires[i - 1]));
                result[i] = Builder.And(Wires[i], condition);
            }

            return new SecureWord(Builder, result);
        }

        public static SecureWord And(params SecureWord[] values)
        {
            return values.AggregateDepthEfficient((x, y) => x & y);
        }
        
        public static SecureWord Or(params SecureWord[] values)
        {
            return values.AggregateDepthEfficient((x, y) => x | y);
        }

        public static SecureWord Xor(params SecureWord[] values)
        {
            return values.AggregateDepthEfficient((x, y) => x ^ y);
        }
        
        public static SecureWord Multiplex(SecureWord left, SecureWord right, SecureBoolean condition)
        {
            if (left.Builder != right.Builder || condition.Builder != right.Builder)
                throw new ArgumentException("Secure words must use the same circuit builder for constructing gates.");

            if (left.Length != right.Length)
                throw new ArgumentException("Secure words must be of same length for multiplexing.");

            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < right.Length; ++i)
            {
                result[i] = right.Builder.Xor(
                    left.Wires[i],
                    right.Builder.And(
                        condition.Wire,
                        right.Builder.Xor(left.Wires[i], right.Wires[i])
                    )
                );
            }

            return new SecureWord(right.Builder, result);
        }

        public static SecureWord operator &(SecureWord left, SecureWord right)
        {
            if (left.Builder != right.Builder)
                throw new ArgumentException("Secure words must use the same circuit builder for constructing gates.");

            if (left.Length != right.Length)
                throw new ArgumentException("Secure words must be of same length for bitwise logical conjunction.");
            
            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = right.Builder.And(left.Wires[i], right.Wires[i]);

            return new SecureWord(right.Builder, result);
        }

        public static SecureWord operator ^(SecureWord left, SecureWord right)
        {
            if (left.Builder != right.Builder)
                throw new ArgumentException("Secure words must use the same circuit builder for constructing gates.");

            if (left.Length != right.Length)
                throw new ArgumentException("Secure words must be of same length for bitwise logical XOR.");
            
            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = right.Builder.Xor(left.Wires[i], right.Wires[i]);

            return new SecureWord(right.Builder, result);
        }

        public static SecureWord operator |(SecureWord left, SecureWord right)
        {
            if (left.Builder != right.Builder)
                throw new ArgumentException("Secure words must use the same circuit builder for constructing gates.");

            if (left.Length != right.Length)
                throw new ArgumentException("Secure words must be of same length for bitwise logical disjunction.");
            
            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = right.Builder.Or(left.Wires[i], right.Wires[i]);

            return new SecureWord(right.Builder, result);
        }

        public static SecureWord operator ~(SecureWord right)
        {
            Wire[] result = new Wire[right.Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = right.Builder.Not(right.Wires[i]);

            return new SecureWord(right.Builder, result);
        }

        public static SecureBoolean operator ==(SecureWord left, SecureWord right)
        {
            if (left.Builder != right.Builder)
                throw new ArgumentException("Secure words must use the same circuit builder for constructing gates.");
            
            CircuitBuilder builder = right.Builder;

            if (left.Length != right.Length)
                return SecureBoolean.False(builder);

            Wire result = left.Wires
                .Zip(right.Wires, (leftWire, rightWire) => builder.Not(builder.Xor(leftWire, rightWire)))
                .ToArray()
                .AggregateDepthEfficient((x, y) => builder.And(x, y));

            return new SecureBoolean(right.Builder, result);
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
    }
}
