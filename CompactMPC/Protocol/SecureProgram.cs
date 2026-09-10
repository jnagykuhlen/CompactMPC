using System.Collections.Generic;
using CompactMPC.Networking;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public abstract class SecureProgram
{
    protected abstract void Compile(ISecureProgramContext context);

    public CompiledSecureProgram Compile(MultiPartySessionDescription sessionDescription)
    {
        var context = new SecureProgramContext(sessionDescription);
        Compile(context);
        return context.CreateCompiledSecureProgram();
    }
}

public interface ISecureProgramContext
{
    IReadOnlyList<TExpression> Share<TExpression>(Input<TExpression> input, IReadOnlySet<Role> roles) where TExpression : IExpression;
    TExpression ShareSingle<TExpression>(Input<TExpression> input, Role role) where TExpression : IExpression;
    void Reveal<TExpression>(Output<TExpression> output, TExpression expression, IReadOnlySet<Role> roles) where TExpression : IExpression;
}
