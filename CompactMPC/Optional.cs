using System;

namespace CompactMPC
{
    public readonly struct Optional<T>
    {
        public static readonly Optional<T> Empty = new Optional<T>(default!, false);
        
        private readonly T _value;

        public bool IsPresent { get; }
        
        private Optional(T value, bool isPresent)
        {
            _value = value;
            IsPresent = isPresent;
        }
        
        public static Optional<T> FromValue(T value)
        {
            return new Optional<T>(value, true);
        }
        
        public T Value
        {
            get
            {
                return _value ?? throw new NotSupportedException("Cannot get value from empty optional.");
            }
        }
    }
}
