using System.Collections.Generic;

namespace CompactMPC.Protocol;

public abstract class SecureProgram
{
    public abstract void Compile(ISecureProgramContext context);
}

public interface ISecureProgramContext
{
    IReadOnlyList<TExpression> Share<TExpression>(Input<TExpression> input) where TExpression : IExpression;
    TExpression ShareSingle<TExpression>(Input<TExpression> input) where TExpression : IExpression;
    void Reveal<TExpression>(Output<TExpression> output, TExpression expression) where TExpression : IExpression;
}
