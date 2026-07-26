namespace CompactMPC.Collections;

public class ReadOnlyListWriter<T>(IWriteOnlyList<T> list)
{
    private int _position;

    public IWriteOnlyList<T> NextSlice(int count)
    {
        var slice = list.WriteOnlySlice(_position, count);
        _position += count;
        return slice;
    }
}
