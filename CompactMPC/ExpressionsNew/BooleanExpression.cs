using CompactMPC.ExpressionsNew.Internal;

namespace CompactMPC.ExpressionsNew
{
    public class BooleanExpression : Expression<bool>
    {
        public static readonly BooleanExpression False = new BooleanExpression(Wire.Zero);
        public static readonly BooleanExpression True = new BooleanExpression(Wire.One);

        public BooleanExpression(Wire wire)
            : base(new[] {wire}) { }

        public static BooleanExpression FromInput()
        {
            return new BooleanExpression(Wire.Input());
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

        public override IBitConverter<bool> Converter
        {
            get
            {
                return BooleanBitConverter.Instance;
            }
        }
    }
}
