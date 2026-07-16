using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Circuits.New;
using CompactMPC.Collections;
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

        var perPartyInputLocalShares = await SendInputRemoteSharesAsync(context, programInput).AndThenAll(
            MultiPartySession.RemotePartySessions.Select(session => ReceiveInputLocalSharesAsync(session, context))
        );

        var circuitEvaluator = new SecretSharingBooleanCircuitEvaluator(MultiPartySession, multiplicativeSharing);
        var circuitEvaluation = ForwardCircuitEvaluation.From(circuitEvaluator);

        foreach (var (party, localShares) in perPartyInputLocalShares)
        {
            circuitEvaluation.Input(
                context.GetPerPartyInput(party).ExpressionDescriptions
                    .SelectMany(expressionDescription => expressionDescription.Expression.Wires)
                    .Select((wire, index) => new WireValue<Task<Bit>>(wire, Task.FromResult(localShares[index])))
            );
        }

        circuitEvaluation.Output(context.GetOutputs().Wires);

        var circuitEvaluationResult = circuitEvaluation.Execute();

        var perPartyOutputShares = await SendOutputLocalSharesAsync(context, circuitEvaluationResult).AndThenAll(
            MultiPartySession.RemotePartySessions.Select(session => ReceiveOutputRemoteSharesAsync(session, context))
        );

        var outputBits = BitArray.Xor(perPartyOutputShares);
        return new SecureProgramOutput(output =>
            {
                var position = 0;
                foreach (var expressionDescription in context.GetOutputs().ExpressionDescriptions)
                {
                    var numberOfBits = expressionDescription.Expression.Wires.Count;

                    if (expressionDescription.Output == output)
                        return (expressionDescription.Expression, outputBits.ReadOnlySlice(position, numberOfBits));

                    position += numberOfBits;
                }

                throw new InvalidOperationException("Output not found.");
            }
        );
    }

    private async Task<(Party, BitArray)> ReceiveInputLocalSharesAsync(ITwoPartyNetworkSession session, SecureProgramContext context)
    {
        var message = await session.Channel.ReadMessageAsync();
        return (
            session.RemoteParty,
            BitArray.FromBytes(message.ToBuffer(), context.GetPerPartyInput(session.RemoteParty).TotalNumberOfBits)
        );
    }

    private async Task<(Party, BitArray)> SendInputRemoteSharesAsync(SecureProgramContext context, SecureProgramInput programInput)
    {
        var localShares = context.GetPerPartyInput(MultiPartySession.LocalParty).GetBits(programInput);

        foreach (var session in MultiPartySession.RemotePartySessions)
        {
            var remoteShares = RandomNumberGenerator.GetBits(localShares.Length);
            localShares.Xor(remoteShares);

            await session.Channel.WriteMessageAsync(new Message(remoteShares.ToBytes()));
        }

        return (MultiPartySession.LocalParty, localShares);
    }

    private async Task<BitArray> ReceiveOutputRemoteSharesAsync(ITwoPartyNetworkSession session, SecureProgramContext context)
    {
        Console.WriteLine($"RECEIVING OUTPUT SHARES: {context.GetOutputs().TotalNumberOfBits} BITS");
        
        var message = await session.Channel.ReadMessageAsync();
        return BitArray.FromBytes(message.ToBuffer(), context.GetOutputs().TotalNumberOfBits);
    }

    private async Task<BitArray> SendOutputLocalSharesAsync(SecureProgramContext context, ForwardCircuitEvaluationResult<Task<Bit>> circuitEvaluationResult)
    {
        var localShares = new BitArray(
            await Task.WhenAll(context.GetOutputs().Wires.Select(circuitEvaluationResult.Value))
        );

        foreach (var session in MultiPartySession.RemotePartySessions)
            await session.Channel.WriteMessageAsync(new Message(localShares.ToBytes()));

        return localShares;
    }

    public SecureComputationRun<TProgram> Run<TProgram>(TProgram program) where TProgram : SecureProgram =>
        new(this, program);

    private class SecureProgramContext(IMultiPartyNetworkSession multiPartySession) : ISecureProgramContext
    {
        private readonly Dictionary<Party, PerPartyInput> _perPartyInputs =
            multiPartySession.Parties.ToDictionary(party => party, _ => new PerPartyInput());

        private readonly PerPartyOutput _outputs = new();

        public IReadOnlyList<TExpression> Share<TExpression>(Input<TExpression> input) where TExpression : IExpression
        {
            var expressions = new List<TExpression>(multiPartySession.NumberOfParties);

            foreach (var party in multiPartySession.Parties.OrderBy(party => party.Guid))
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

        public void Reveal<TExpression>(Output<TExpression> output, TExpression expression) where TExpression : IExpression =>
            _outputs.AddExpression(output, expression);

        public IPerPartyInput GetPerPartyInput(Party party) => _perPartyInputs[party];
        public PerPartyOutput GetOutputs() => _outputs;

        private class PerPartyInput : IPerPartyInput
        {
            private int _totalNumberOfBits;
            private readonly List<InputExpressionDescription> _expressionDescriptions = new();

            public void AddExpression<TExpression>(Input<TExpression> input, TExpression expression) where TExpression : IExpression
            {
                _totalNumberOfBits += expression.Wires.Count;
                _expressionDescriptions.Add(
                    new InputExpressionDescription(expression, programInput => programInput.GetValue(input, expression))
                );
            }

            public int TotalNumberOfBits => _totalNumberOfBits;
            public IReadOnlyList<InputExpressionDescription> ExpressionDescriptions => _expressionDescriptions;
        }
    }

    private interface IPerPartyInput
    {
        int TotalNumberOfBits { get; }
        IReadOnlyList<InputExpressionDescription> ExpressionDescriptions { get; }

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

    private class InputExpressionDescription(IExpression expression, Func<SecureProgramInput, IInputValue> inputValueSelector)
    {
        public IInputValue GetInputValue(SecureProgramInput programInput) => inputValueSelector(programInput);
        public IExpression Expression { get; } = expression;
    }

    private class PerPartyOutput
    {
        private int _totalNumberOfBits;
        private readonly List<OutputExpressionDescription> _expressionDescriptions = new();

        public void AddExpression<TExpression>(Output<TExpression> output, TExpression expression) where TExpression : IExpression
        {
            _totalNumberOfBits += expression.Wires.Count;
            _expressionDescriptions.Add(new OutputExpressionDescription(output, expression));
        }

        public int TotalNumberOfBits => _totalNumberOfBits;
        public IReadOnlyList<OutputExpressionDescription> ExpressionDescriptions => _expressionDescriptions;
        public IEnumerable<Wire> Wires => ExpressionDescriptions.SelectMany(expressionDescription => expressionDescription.Expression.Wires).ToArray();
    }

    private class OutputExpressionDescription(object output, IExpression expression)
    {
        public object Output { get; } = output;
        public IExpression Expression { get; } = expression;
    }
}
