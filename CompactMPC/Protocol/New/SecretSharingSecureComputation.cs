using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.ExpressionsNew;
using CompactMPC.Networking;

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
            private readonly List<IExpressionDescription> _expressionDescriptions = new();

            public void AddExpression<TExpression>(Input<TExpression> input, TExpression expression) where TExpression : IExpression
            {
                _totalNumberOfBits += expression.Wires.Count;
                _expressionDescriptions.Add(new ExpressionDescription<TExpression>(input, expression));
            }

            public int TotalNumberOfBits => _totalNumberOfBits;
            public IReadOnlyList<IExpressionDescription> ExpressionDescriptions => _expressionDescriptions;
        }

        private class ExpressionDescription<TExpression>(Input<TExpression> input, TExpression expression) : IExpressionDescription
            where TExpression : IExpression
        {
            public void WriteBits(SecureProgramInput programInput, BitArray destination, int position) =>
                programInput.WriteBits(input, expression, destination, position);

            public IExpression Expression { get; } = expression;
        }
    }

    private interface IPerPartyInput
    {
        int TotalNumberOfBits { get; }
        IReadOnlyList<IExpressionDescription> ExpressionDescriptions { get; }

        BitArray GetBits(SecureProgramInput programInput)
        {
            var bits = new BitArray(TotalNumberOfBits);
            var position = 0;

            foreach (var expressionDescription in ExpressionDescriptions)
            {
                expressionDescription.WriteBits(programInput, bits, position);
                position += expressionDescription.Expression.Wires.Count;
            }
            
            return bits;
        }
    }

    private interface IExpressionDescription
    {
        void WriteBits(SecureProgramInput programInput, BitArray destination, int position);
        IExpression Expression { get; }
    }
}
