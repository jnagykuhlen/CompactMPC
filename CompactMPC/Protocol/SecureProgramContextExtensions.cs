using System;
using System.Collections.Generic;
using CompactMPC.Networking;

namespace CompactMPC.Protocol;

public static class SecureProgramContextExtensions
{
    private static readonly RoleMatcher MatchAllRoles = _ => true;

    public static IReadOnlyList<TExpression> Share<TExpression>(this ISecureProgramContext context, Input<TExpression> input) where TExpression : IExpression =>
        context.Share(input, MatchAllRoles);

    public static IReadOnlyList<TExpression> Share<TExpression>(this ISecureProgramContext context, Input<TExpression> input, Role role) where TExpression : IExpression =>
        context.Share(input, MatchSingleRole(role));

    public static IReadOnlyList<TExpression> Share<TExpression>(this ISecureProgramContext context, Input<TExpression> input, params Role[] roles) where TExpression : IExpression =>
        context.Share(input, MatchAnyRole(roles));

    public static void Reveal<TExpression>(this ISecureProgramContext context, Output<TExpression> output, TExpression expression) where TExpression : IExpression =>
        context.Reveal(output, expression, MatchAllRoles);

    public static void Reveal<TExpression>(this ISecureProgramContext context, Output<TExpression> output, TExpression expression, Role role) where TExpression : IExpression =>
        context.Reveal(output, expression, MatchSingleRole(role));

    public static void Reveal<TExpression>(this ISecureProgramContext context, Output<TExpression> output, TExpression expression, params Role[] roles) where TExpression : IExpression =>
        context.Reveal(output, expression, MatchAnyRole(roles));

    private static RoleMatcher MatchSingleRole(Role roleToMatch) => role => role == roleToMatch;
    private static RoleMatcher MatchAnyRole(params Role[] rolesToMatch) => role => rolesToMatch.Contains(role);
}
