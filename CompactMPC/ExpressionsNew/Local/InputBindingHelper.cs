namespace CompactMPC.ExpressionsNew.Local
{
    public static class InputBindingHelper
    {
        public static InputBinding<T> Bind<T>(this IInputExpression<T> expression, T value)
        {
            return new InputBinding<T>(expression, value);
        }
    }
}