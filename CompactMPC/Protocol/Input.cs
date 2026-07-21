using System;

namespace CompactMPC.Protocol;

public class Input<T>(Func<T> createFunction): IInput<T>
{
    public T Create() => createFunction();
}

public interface IInput<out T>
{
    T Create();
}
