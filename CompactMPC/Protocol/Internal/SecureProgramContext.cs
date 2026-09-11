using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Protocol.Internal;

public class SecureProgramContext(MultiPartySessionDescription sessionDescription) : ISecureProgramContext
{
    private readonly Dictionary<PartySlot, PartyInputContext> _inputContextsByPartySlot =
        sessionDescription.PartySlots.ToDictionary(partySlot => partySlot, _ => new PartyInputContext());

    private readonly PartyOutputContext _outputContext = new();

    public IReadOnlyList<TExpression> Share<TExpression>(Input<TExpression> input, RoleMatcher roleMatcher) where TExpression : IExpression
    {
        var expressions = new List<TExpression>(sessionDescription.NumberOfParties);

        foreach (var partySlot in sessionDescription.PartySlots)
        {
            var expression = input.Create();
            _inputContextsByPartySlot[partySlot].Add(input, expression);
            expressions.Add(expression);
        }

        return expressions;
    }

    public void Reveal<TExpression>(Output<TExpression> output, TExpression expression, RoleMatcher roleMatcher) where TExpression : IExpression =>
        _outputContext.Add(output, expression);

    public CompiledSecureProgram CreateCompiledSecureProgram() =>
        new(sessionDescription, _inputContextsByPartySlot, _outputContext);
}
