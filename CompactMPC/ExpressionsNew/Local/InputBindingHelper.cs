namespace CompactMPC.ExpressionsNew.Local
{
    public static class InputBindingHelper
    {
        public static InputBinding Bind<T>(this IInputExpression<T> expression, T value)
        {
            return InputBinding.From(expression, value);
        }
    }
}