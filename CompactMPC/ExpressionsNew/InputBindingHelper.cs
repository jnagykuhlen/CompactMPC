namespace CompactMPC.ExpressionsNew
{
    public static class InputBindingHelper
    {
        public static InputBinding<T> Bind<T>(this Expression<T> expression, T value)
        {
            return new InputBinding<T>(expression, value);
        }
    }
}