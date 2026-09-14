using System.Collections.Generic;
using System.Linq;
using CompactMPC.Collections;

namespace CompactMPC.Protocol.Internal;

public class SecureProgramContext(MultiPartySessionDescription sessionDescription) : ISecureProgramContext
{
    private readonly Dictionary<PartySlot, PartyContext> _partyContexts =
        sessionDescription.PartySlots.ToDictionary(partySlot => partySlot, _ => new PartyContext());

    public IReadOnlyList<TExpression> Share<TExpression>(Input<TExpression> input, RoleMatcher roleMatcher) where TExpression : IExpression
    {
        var expressions = new List<TExpression>(sessionDescription.NumberOfParties);

        foreach (var partySlot in sessionDescription.PartySlots)
        {
            var expression = input.Create();
            _partyContexts[partySlot].Add(input, expression);
            expressions.Add(expression);
        }

        return expressions;
    }

    public void Reveal<TExpression>(Output<TExpression> output, TExpression expression, RoleMatcher roleMatcher) where TExpression : IExpression
    {
        foreach (var partySlot in sessionDescription.PartySlots)
            _partyContexts[partySlot].Add(output, expression);
    }
    
    public CompiledSecureProgram CreateCompiledSecureProgram() =>
        new(sessionDescription, _partyContexts.ToDictionary(partyContext => partyContext.CreateCompiledPartyContext()));

    private class PartyContext
    {
        private readonly List<InputExpressionDescription> _inputExpressions = new();
        private readonly List<OutputExpressionDescription> _outputExpressions = new();

        public void Add<TExpression>(Input<TExpression> input, TExpression expression) where TExpression : IExpression =>
            _inputExpressions.Add(
                new InputExpressionDescription(
                    expression,
                    programInput => programInput.GetValue(input, expression)
                )
            );

        public void Add<TExpression>(Output<TExpression> output, TExpression expression) where TExpression : IExpression =>
            _outputExpressions.Add(new OutputExpressionDescription((IOutput<IExpression>)output, expression));

        public CompiledPartyContext CreateCompiledPartyContext() =>
            new(new CompiledPartyInputContext(_inputExpressions), new CompiledPartyOutputContext(_outputExpressions));
    }
}
