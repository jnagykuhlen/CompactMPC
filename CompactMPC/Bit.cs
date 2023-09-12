﻿namespace CompactMPC
{
    public readonly struct Bit
    {
        public static readonly Bit Zero = new Bit(0);
        public static readonly Bit One = new Bit(1);

        private readonly byte _value;

        public Bit(bool value)
        {
            _value = (byte)(value ? 1 : 0);
        }

        public Bit(byte value)
        {
            _value = (byte)(value & 1);
        }

        public override bool Equals(object? other)
        {
            if (other is Bit otherBit)
                return _value == otherBit._value;

            return false;
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public override string ToString()
        {
            return IsSet ? "1" : "0";
        }

        public bool IsSet
        {
            get
            {
                return _value != 0;
            }
        }

        public static bool operator ==(Bit left, Bit right)
        {
            return left._value == right._value;
        }

        public static bool operator !=(Bit left, Bit right)
        {
            return left._value != right._value;
        }

        public static Bit operator |(Bit left, Bit right)
        {
            return new Bit((byte)(left._value | right._value));
        }

        public static Bit operator ^(Bit left, Bit right)
        {
            return new Bit((byte)(left._value ^ right._value));
        }

        public static Bit operator &(Bit left, Bit right)
        {
            return new Bit((byte)(left._value & right._value));
        }

        public static Bit operator ~(Bit right)
        {
            return new Bit((byte)~right._value);
        }
        
        public static bool operator true(Bit right)
        {
            return right._value != 0;
        }

        public static bool operator false(Bit right)
        {
            return right._value == 0;
        }

        public static explicit operator byte(Bit right)
        {
            return right._value;
        }

        public static explicit operator bool(Bit right)
        {
            return right._value != 0;
        }
        
        public static explicit operator Bit(byte right)
        {
            return new Bit(right);
        }

        public static explicit operator Bit(bool right)
        {
            return new Bit(right);
        }
    }
}
