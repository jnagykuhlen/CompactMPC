namespace CompactMPC.Protocol.New;

public abstract class SecureProgram
{
    public abstract void Compile(ISecureProgramContext context);
}

public interface ISecureProgramContext
{
    T[] Share<T>(Input<T> input);
    T ShareSingle<T>(Input<T> input);
    void Reveal<T>(Output<T> output, T expression);
}
