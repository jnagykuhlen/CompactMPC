using System.Collections.Generic;
using CompactMPC.Circuits.New;
using CompactMPC.ExpressionsNew.Internal;

namespace CompactMPC.ExpressionsNew
{
    public class BooleanExpression : Expression, IInputExpression<bool>, IOutputExpression<bool>
    {
        public static readonly BooleanExpression False = new BooleanExpression(Wire.Zero);
        public static readonly BooleanExpression True = new BooleanExpression(Wire.One);

        public BooleanExpression(Wire wire)
            : base(new[] { wire }) { }

        public IReadOnlyList<Bit> ToBits(bool value)
        {
            return BooleanBitConverter.Instance.ToBits(value, 1);

        }

        public bool FromBits(IReadOnlyList<Bit> bits)
        {
            return BooleanBitConverter.Instance.FromBits(bits);
        }

        public static BooleanExpression Assignable()
        {
            return new BooleanExpression(Wire.Assignable());
        }

        public static BooleanExpression operator &(BooleanExpression left, BooleanExpression right)
        {
            return new BooleanExpression(Wire.And(left.Wire, right.Wire));
        }

        public static BooleanExpression operator ^(BooleanExpression left, BooleanExpression right)
        {
            return new BooleanExpression(Wire.Xor(left.Wire, right.Wire));
        }

        public static BooleanExpression operator |(BooleanExpression left, BooleanExpression right)
        {
            return new BooleanExpression(Wire.Or(left.Wire, right.Wire));
        }

        public static BooleanExpression operator !(BooleanExpression right)
        {
            return new BooleanExpression(Wire.Not(right.Wire));
        }

        public static bool operator false(BooleanExpression right)
        {
            return right.Wire == Wire.Zero;
        }

        public static bool operator true(BooleanExpression right)
        {
            return right.Wire == Wire.One;
        }

        public Wire Wire
        {
            get
            {
                return Wires[0];
            }
        }
    }
}
