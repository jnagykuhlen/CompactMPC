namespace CompactMPC.ExpressionsNew.Local
{
    public static class ExpressionInputBindingHelper
    {
        public static ExpressionInputBinding Bind<T>(this IInputExpression<T> expression, T value)
        {
            return ExpressionInputBinding.From(expression, value);
        }
    }
}