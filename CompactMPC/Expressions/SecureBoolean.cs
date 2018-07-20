using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;

namespace CompactMPC.Expressions
{
    public class SecureBoolean : SecurePrimitive
    {
        private Wire _wire;

        public SecureBoolean(CircuitBuilder builder, Wire wire)
            : base(builder, new[] { wire })
        {
            _wire = wire;
        }

        public static SecureBoolean True(CircuitBuilder builder)
        {
            return new SecureBoolean(builder, Wire.One);
        }

        public static SecureBoolean False(CircuitBuilder builder)
        {
            return new SecureBoolean(builder, Wire.Zero);
        }

        public static SecureBoolean operator &(SecureBoolean left, SecureBoolean right)
        {
            if (left.Builder != right.Builder)
                throw new ArgumentException("Secure booleans must use the same circuit builder for constructing gates.");
            
            return new SecureBoolean(right.Builder, right.Builder.And(left.Wire, right.Wire));
        }

        public static SecureBoolean operator ^(SecureBoolean left, SecureBoolean right)
        {
            if (left.Builder != right.Builder)
                throw new ArgumentException("Secure booleans must use the same circuit builder for constructing gates.");
            
            return new SecureBoolean(right.Builder, right.Builder.Xor(left.Wire, right.Wire));
        }

        public static SecureBoolean operator |(SecureBoolean left, SecureBoolean right)
        {
            if (left.Builder != right.Builder)
                throw new ArgumentException("Secure booleans must use the same circuit builder for constructing gates.");

            return new SecureBoolean(right.Builder, right.Builder.Or(left.Wire, right.Wire));
        }

        public static SecureBoolean operator !(SecureBoolean right)
        {
            return new SecureBoolean(right.Builder, right.Builder.Not(right.Wire));
        }
        
        public static bool operator true(SecureBoolean right)
        {
            return right.Wire == Wire.One;
        }

        public static bool operator false(SecureBoolean right)
        {
            return right.Wire == Wire.Zero;
        }

        public Wire Wire
        {
            get
            {
                return _wire;
            }
        }
    }
}
