namespace CompactMPC.Collections;

public interface IWriteOnlyList<in T>
{
    T this[int index] { set; }
    int Count { get; }
}
