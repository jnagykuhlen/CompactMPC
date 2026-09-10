using System.Collections.Generic;
using CompactMPC.Networking;

namespace CompactMPC.Protocol;

public static class SecureProgramContextExtensions
{
    private static readonly IReadOnlySet<Role> AllRoles = new HashSet<Role>();

    public static IReadOnlyList<TExpression> Share<TExpression>(this ISecureProgramContext context, Input<TExpression> input) where TExpression : IExpression =>
        context.Share(input, AllRoles);
    
    public static void Reveal<TExpression>(this ISecureProgramContext context, Output<TExpression> output, TExpression expression) where TExpression : IExpression =>
        context.Reveal(output, expression, AllRoles);
}
