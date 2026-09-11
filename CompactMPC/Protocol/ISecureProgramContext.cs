using System.Collections.Generic;
using CompactMPC.Networking;

namespace CompactMPC.Protocol;

public interface ISecureProgramContext
{
    IReadOnlyList<TExpression> Share<TExpression>(Input<TExpression> input, RoleMatcher roleMatcher) where TExpression : IExpression;
    void Reveal<TExpression>(Output<TExpression> output, TExpression expression, RoleMatcher roleMatcher) where TExpression : IExpression;
}

public delegate bool RoleMatcher(Role role);
