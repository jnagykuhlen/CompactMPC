using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Circuits.New;
using CompactMPC.Collections;
using CompactMPC.Cryptography;
using CompactMPC.Networking;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol.New;

public class SecretSharingSecureComputation(IMultiPartyNetworkSession multiPartySession, IMultiplicativeSharing multiplicativeSharing)
{
    public IMultiPartyNetworkSession MultiPartySession { get; } = multiPartySession;

    public SecureComputationRun<TProgram> Run<TProgram>(TProgram program) where TProgram : SecureProgram =>
        new(this, program);

    public async Task<SecureProgramOutput> RunAsync(SecureProgram program, SecureProgramInput programInput)
    {
        var context = new SecureProgramContext(MultiPartySession);
        program.Compile(context);

        var perPartyInputShares = await SendInputRemoteSharesAsync(context, programInput).AndThenAll(
            MultiPartySession.RemotePartySessions.Select(session => ReceiveInputLocalSharesAsync(session, context))
        );

        var circuitEvaluator = new SecretSharingAsyncBatchCircuitEvaluator(MultiPartySession, multiplicativeSharing);
        var evaluationResult = await new ForwardCircuitEvaluation<Bit>(circuitEvaluator)
            .ExecuteAsync(GetInputWireValues(context, perPartyInputShares), GetOutputWires(context));

        var perPartyOutputShares = await SendOutputLocalSharesAsync(context, evaluationResult).AndThenAll(
            MultiPartySession.RemotePartySessions.Select(session => ReceiveOutputRemoteSharesAsync(session, context))
        );

        return CreateProgramOutput(context, perPartyOutputShares);
    }

    private static IEnumerable<WireValue<Bit>> GetInputWireValues(SecureProgramContext context, PerPartyShares[] perPartyInputShares) =>
        perPartyInputShares
            .OrderBy(shares => shares.Party.Guid)
            .SelectMany(shares => context.GetInputContext(shares.Party).ExpressionDescriptions
                .SelectMany(description => description.Expression.Wires)
                .Select((wire, index) => new WireValue<Bit>(wire, shares.Shares[index]))
            );

    private static IEnumerable<Wire> GetOutputWires(SecureProgramContext context) => context.GetOutputContext().Wires;

    private static SecureProgramOutput CreateProgramOutput(SecureProgramContext context, BitArray[] perPartyOutputShares)
    {
        var outputBits = BitArray.Xor(perPartyOutputShares);
        var outputBitsReader = outputBits.GetReader();
        return new SecureProgramOutput(
            context.GetOutputContext().ExpressionDescriptions
                .ToDictionary(
                    description => description.Output,
                    description => (description.Expression, outputBitsReader.NextSlice(description.Expression.Wires.Count))
                )
        );
    }

    private async Task<PerPartyShares> ReceiveInputLocalSharesAsync(ITwoPartyNetworkSession session, SecureProgramContext context)
    {
        var message = await session.Channel.ReadMessageAsync();
        return new PerPartyShares(
            session.RemoteParty,
            BitArray.FromBytes(message.ToBuffer(), context.GetInputContext(session.RemoteParty).TotalNumberOfBits)
        );
    }

    private async Task<PerPartyShares> SendInputRemoteSharesAsync(SecureProgramContext context, SecureProgramInput programInput)
    {
        var localShares = context.GetInputContext(MultiPartySession.LocalParty).GetInputBits(programInput);

        foreach (var session in MultiPartySession.RemotePartySessions)
        {
            var remoteShares = RandomNumberGenerator.GetBits(localShares.Length);
            localShares.Xor(remoteShares);
            await session.Channel.WriteMessageAsync(new Message(remoteShares.ToBytes()));
        }

        return new PerPartyShares(MultiPartySession.LocalParty, localShares);
    }

    private static async Task<BitArray> ReceiveOutputRemoteSharesAsync(ITwoPartyNetworkSession session, SecureProgramContext context)
    {
        var message = await session.Channel.ReadMessageAsync();
        return BitArray.FromBytes(message.ToBuffer(), context.GetOutputContext().TotalNumberOfBits);
    }

    private async Task<BitArray> SendOutputLocalSharesAsync(SecureProgramContext context, ForwardCircuitEvaluationResult<Bit> evaluationResult)
    {
        var localShares = new BitArray(
            context.GetOutputContext().Wires.Select(evaluationResult.Value).ToArray()
        );

        foreach (var session in MultiPartySession.RemotePartySessions)
            await session.Channel.WriteMessageAsync(new Message(localShares.ToBytes()));

        return localShares;
    }

    private record PerPartyShares(Party Party, BitArray Shares);
}
