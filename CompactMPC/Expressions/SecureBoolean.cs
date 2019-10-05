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
        public static readonly SecureBoolean False = new SecureBoolean(Wire.Zero);
        public static readonly SecureBoolean True = new SecureBoolean(Wire.One);

        private Wire _wire;

        public SecureBoolean(Wire wire)
        {
            _wire = wire;
        }

        public override bool Equals(object other)
        {
            return other is SecureBoolean && _wire.Equals(((SecureBoolean)other).Wire);
        }

        public override int GetHashCode()
        {
            return _wire.GetHashCode();
        }

        public static SecureBoolean operator ==(SecureBoolean left, SecureBoolean right)
        {
            return new SecureBoolean(Wire.Not(Wire.Xor(left.Wire, right.Wire)));
        }

        public static SecureBoolean operator !=(SecureBoolean left, SecureBoolean right)
        {
            return new SecureBoolean(Wire.Xor(left.Wire, right.Wire));
        }

        public static SecureBoolean operator &(SecureBoolean left, SecureBoolean right)
        {
            return new SecureBoolean(Wire.And(left.Wire, right.Wire));
        }

        public static SecureBoolean operator ^(SecureBoolean left, SecureBoolean right)
        {
            return new SecureBoolean(Wire.Xor(left.Wire, right.Wire));
        }

        public static SecureBoolean operator |(SecureBoolean left, SecureBoolean right)
        {
            return new SecureBoolean(Wire.Or(left.Wire, right.Wire));
        }

        public static SecureBoolean operator !(SecureBoolean right)
        {
            return new SecureBoolean(Wire.Not(right.Wire));
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

        public override IReadOnlyList<Wire> Wires
        {
            get
            {
                return new[] { _wire };
            }
        }
    }
}
