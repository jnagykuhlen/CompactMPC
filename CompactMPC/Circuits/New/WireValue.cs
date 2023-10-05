using System;

namespace CompactMPC.Circuits.New
{
    public static class WireValue
    {
        public static WireValue<T> Create<T>(Wire wire, T value)
        {
            return new WireValue<T>(wire, value);
        }
    }
    
    public class WireValue<T>
    {
        public WireValue(Wire wire, T value)
        {
            if (!wire.IsAssignable)
                throw new ArgumentException("Cannot assign input to unassignable wire.", nameof(wire));
            
            Wire = wire;
            Value = value;
        }

        public Wire Wire { get; }
        public T Value { get; }
    }
}