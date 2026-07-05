using System;

namespace CompactMPC.Protocol.New;

public class Input<T>(Func<T> createFunction): IInput<T>
{
    public T Create() => createFunction();
}

public interface IInput<out T>
{
    T Create();
}