namespace CompactMPC;

public readonly struct Bit
{
    public static readonly Bit Zero = new(0);
    public static readonly Bit One = new(1);

    private readonly byte _value;

    public Bit(bool isSet) => _value = (byte)(isSet ? 1 : 0);
    public Bit(byte value) => _value = (byte)(value & 1);

    public bool IsSet => _value != 0;

    public override bool Equals(object? other) => other is Bit otherBit && _value == otherBit._value;
    public override int GetHashCode() => _value;
    public override string ToString() => IsSet ? "1" : "0";

    public static bool operator ==(Bit left, Bit right) => left._value == right._value;
    public static bool operator !=(Bit left, Bit right) => left._value != right._value;
    public static Bit operator |(Bit left, Bit right) => new((byte)(left._value | right._value));
    public static Bit operator ^(Bit left, Bit right) => new((byte)(left._value ^ right._value));
    public static Bit operator &(Bit left, Bit right) => new((byte)(left._value & right._value));
    public static Bit operator ~(Bit right) => new((byte)~right._value);
    public static bool operator true(Bit right) => right._value != 0;
    public static bool operator false(Bit right) => right._value == 0;

    public static explicit operator byte(Bit right) => right._value;
    public static explicit operator bool(Bit right) => right._value != 0;
    public static explicit operator Bit(byte right) => new(right);
    public static explicit operator Bit(bool right) => new(right);
}
