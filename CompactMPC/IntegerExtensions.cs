namespace CompactMPC;

public static class IntegerExtensions
{
    extension(int value)
    {
        public bool IsOdd => (value & 1) != 0;
    }
}
