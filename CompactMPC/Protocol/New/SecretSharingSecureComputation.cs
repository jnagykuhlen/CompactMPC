using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Cryptography;
using CompactMPC.ExpressionsNew;
using CompactMPC.Networking;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol.New;

public class SecretSharingSecureComputation(IMultiPartyNetworkSession multiPartySession, IMultiplicativeSharing multiplicativeSharing)
{
    public IMultiPartyNetworkSession MultiPartySession { get; } = multiPartySession;

    public async Task<SecureProgramOutput> RunAsync(SecureProgram program, SecureProgramInput programInput)
    {
        var context = new SecureProgramContext(MultiPartySession);
        program.Compile(context);

        var localShares = await Task.WhenAll(
            MultiPartySession.RemotePartySessions
                .Select(session => ReceiveLocalSharesAsync(session, context))
                .Append(SendRemoteSharesAsync(context, programInput))
        );
    }

    private async Task<BitArray> ReceiveLocalSharesAsync(ITwoPartyNetworkSession session, SecureProgramContext context)
    {
        var message = await session.Channel.ReadMessageAsync();
        return BitArray.FromBytes(message.ToBuffer(), context.GetPerPartyInput(session.RemoteParty).TotalNumberOfBits);
    }

    private Task<BitArray> SendRemoteSharesAsync(SecureProgramContext context, SecureProgramInput programInput)
    {
        var unmaskedLocalInput = context.GetPerPartyInput(MultiPartySession.LocalParty).GetBits(programInput);

        // TODO
    }

    public SecureComputationRun<TProgram> Run<TProgram>(TProgram program) where TProgram : SecureProgram =>
        new(this, program);

    private class SecureProgramContext(IMultiPartyNetworkSession multiPartySession) : ISecureProgramContext
    {
        private readonly Dictionary<Party, PerPartyInput> _perPartyInputs =
            multiPartySession.Parties.ToDictionary(party => party, _ => new PerPartyInput());

        public IReadOnlyList<TExpression> Share<TExpression>(Input<TExpression> input) where TExpression : IExpression
        {
            var expressions = new List<TExpression>(multiPartySession.NumberOfParties);

            foreach (var party in multiPartySession.Parties)
            {
                var expression = input.Create();

                _perPartyInputs[party].AddExpression(input, expression);
                expressions.Add(expression);
            }

            return expressions;
        }

        public TExpression ShareSingle<TExpression>(Input<TExpression> input) where TExpression : IExpression
        {
            throw new NotImplementedException();
        }

        public void Reveal<TExpression>(Output<TExpression> output, TExpression expression) where TExpression : IExpression
        {
            throw new NotImplementedException();
        }

        public IPerPartyInput GetPerPartyInput(Party party) => _perPartyInputs[party];

        private class PerPartyInput : IPerPartyInput
        {
            private int _totalNumberOfBits;
            private readonly List<ExpressionDescription> _expressionDescriptions = new();

            public void AddExpression<TExpression>(Input<TExpression> input, TExpression expression) where TExpression : IExpression
            {
                _totalNumberOfBits += expression.Wires.Count;
                _expressionDescriptions.Add(
                    new ExpressionDescription(expression, programInput => programInput.GetValue(input, expression))
                );
            }

            public int TotalNumberOfBits => _totalNumberOfBits;
            public IReadOnlyList<ExpressionDescription> ExpressionDescriptions => _expressionDescriptions;
        }
    }

    private interface IPerPartyInput
    {
        int TotalNumberOfBits { get; }
        IReadOnlyList<ExpressionDescription> ExpressionDescriptions { get; }

        BitArray GetBits(SecureProgramInput programInput)
        {
            var bits = new BitArray(TotalNumberOfBits);
            var bitsWriter = bits.GetWriter();

            foreach (var expressionDescription in ExpressionDescriptions)
            {
                expressionDescription.GetInputValue(programInput)
                    .WriteTo(bitsWriter.NextSlice(expressionDescription.Expression.Wires.Count));
            }

            return bits;
        }
    }

    private class ExpressionDescription(IExpression expression, Func<SecureProgramInput, IInputValue> inputValueSelector)
    {
        public IInputValue GetInputValue(SecureProgramInput programInput) => inputValueSelector(programInput);
        public IExpression Expression { get; } = expression;
    }
}
