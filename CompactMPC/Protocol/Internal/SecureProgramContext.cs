using System;
using System.Collections.Generic;
using System.Linq;
using CompactMPC.Networking;

namespace CompactMPC.Protocol.Internal;

public class SecureProgramContext(OrderedMultiPartyNetworkSession session) : ISecureProgramContext
{
    private readonly Dictionary<Party, PartyInputContext> _partyInputContexts =
        session.OrderedParties.ToDictionary(party => party, _ => new PartyInputContext());

    private readonly PartyOutputContext _outputContext = new();

    public IReadOnlyList<TExpression> Share<TExpression>(Input<TExpression> input) where TExpression : IExpression
    {
        var expressions = new List<TExpression>(session.NumberOfParties);

        foreach (var party in session.OrderedParties)
        {
            var expression = input.Create();
            _partyInputContexts[party].Add(input, expression);
            expressions.Add(expression);
        }

        return expressions;
    }

    public TExpression ShareSingle<TExpression>(Input<TExpression> input) where TExpression : IExpression =>
        throw new NotImplementedException();

    public void Reveal<TExpression>(Output<TExpression> output, TExpression expression) where TExpression : IExpression =>
        _outputContext.Add(output, expression);

    public PartyInputContext GetInputContext(Party party) => _partyInputContexts[party];
    public PartyOutputContext GetOutputContext() => _outputContext;
}
