using System;
using System.Collections.Generic;
using System.Linq;
using CompactMPC.ExpressionsNew;
using CompactMPC.Networking;
using CompactMPC.Protocol.New;

namespace CompactMPC.Protocol.Internal;

public class SecureProgramContext(IMultiPartyNetworkSession multiPartySession) : ISecureProgramContext
{
    private readonly Dictionary<Party, PartyInputContext> _partyInputContexts =
        multiPartySession.Parties.ToDictionary(party => party, _ => new PartyInputContext());

    private readonly PartyOutputContext _outputContext = new();

    public IReadOnlyList<TExpression> Share<TExpression>(Input<TExpression> input) where TExpression : IExpression
    {
        var expressions = new List<TExpression>(multiPartySession.NumberOfParties);

        foreach (var party in multiPartySession.Parties.OrderBy(party => party.Guid))
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
