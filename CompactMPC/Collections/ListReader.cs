using System.Collections.Generic;

namespace CompactMPC.Collections;

public class ListReader<T>(IReadOnlyList<T> list)
{
    private int _position;

    public IReadOnlyList<T> NextSlice(int count)
    {
        var slice = list.ReadOnlySlice(_position, count);
        _position += count;
        return slice;
    }

    public T Next() => list[_position++];
}
