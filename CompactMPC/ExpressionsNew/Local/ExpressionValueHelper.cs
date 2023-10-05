namespace CompactMPC.ExpressionsNew.Local
{
    public static class ExpressionValueHelper
    {
        public static ExpressionValue Bind<T>(this IInputExpression<T> expression, T value)
        {
            return ExpressionValue.From(expression, value);
        }
    }
}