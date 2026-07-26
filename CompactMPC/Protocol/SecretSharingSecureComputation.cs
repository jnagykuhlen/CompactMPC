using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Circuits;
using CompactMPC.Collections;
using CompactMPC.Cryptography;
using CompactMPC.Networking;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public class SecretSharingSecureComputation(IMultiPartyNetworkSession session, IMultiplicativeSharing multiplicativeSharing)
{
    private readonly OrderedMultiPartyNetworkSession _session = new(session);

    public SecureComputationRun<TProgram> Run<TProgram>(TProgram program) where TProgram : SecureProgram =>
        new(this, program);
    
    public SecureComputationRun<TProgram> Run<TProgram>() where TProgram : SecureProgram, new() =>
        new(this, new TProgram());

    public async Task<SecureProgramOutput> RunAsync(SecureProgram program, SecureProgramInput programInput)
    {
        var context = new SecureProgramContext(_session);
        program.Compile(context);

        var perPartyInputShares = await ExchangeInputSharesAsync(programInput, context);

        var circuitEvaluator = new SecretSharingAsyncBatchCircuitEvaluator(_session, multiplicativeSharing);
        var evaluationResult = await new ForwardCircuitEvaluation<Bit>(circuitEvaluator)
            .ExecuteAsync(GetInputWireValues(context, perPartyInputShares), GetOutputWires(context));

        var perPartyOutputShares = await ExchangeOutputSharesAsync(context, evaluationResult);

        return CreateSecureProgramOutput(context, perPartyOutputShares);
    }

    private Task<PerPartyShares[]> ExchangeInputSharesAsync(SecureProgramInput programInput, SecureProgramContext context)
    {
        var localShares = context.GetInputContext(_session.LocalParty).GetInputBits(programInput);
        return _session.SendAndReceiveAsync(
            multiPartySession => SendInputRemoteSharesAsync(multiPartySession, localShares),
            twoPartySession => ReceiveInputLocalSharesAsync(twoPartySession, context)
        );
    }

    private static async Task<PerPartyShares> SendInputRemoteSharesAsync(IMultiPartyNetworkSession multiPartyNetworkSession, BitArray localShares)
    {
        foreach (var remotePartySession in multiPartyNetworkSession.RemotePartySessions)
        {
            var remoteShares = RandomNumberGenerator.GetBits(localShares.Length);
            localShares.Xor(remoteShares);
            await remotePartySession.Channel.WriteMessageAsync(new Message(remoteShares.ToBytes()));
        }

        return new PerPartyShares(multiPartyNetworkSession.LocalParty, localShares);
    }

    private static async Task<PerPartyShares> ReceiveInputLocalSharesAsync(ITwoPartyNetworkSession session, SecureProgramContext context)
    {
        var message = await session.Channel.ReadMessageAsync();
        return new PerPartyShares(
            session.RemoteParty,
            BitArray.FromBytes(message.ToBuffer(), context.GetInputContext(session.RemoteParty).TotalNumberOfBits)
        );
    }

    private static IEnumerable<WireValue<Bit>> GetInputWireValues(SecureProgramContext context, PerPartyShares[] perPartyInputShares) =>
        perPartyInputShares.SelectMany(shares =>
            context.GetInputContext(shares.Party).ExpressionDescriptions
                .SelectMany(description => description.Expression.Wires)
                .Select((wire, index) => new WireValue<Bit>(wire, shares.Shares[index]))
        );

    private static IEnumerable<Wire> GetOutputWires(SecureProgramContext context) => context.GetOutputContext().NonConstantWires;

    private Task<BitArray[]> ExchangeOutputSharesAsync(SecureProgramContext context, ForwardCircuitEvaluationResult<Bit> evaluationResult)
    {
        var localShares = new BitArray(context.GetOutputContext().NonConstantWires.Select(evaluationResult.Value).ToArray());
        return _session.SendAndReceiveAsync(
            multiPartySession => SendOutputLocalSharesAsync(multiPartySession, localShares),
            twoPartySession => ReceiveOutputRemoteSharesAsync(twoPartySession, context)
        );
    }

    private static async Task<BitArray> SendOutputLocalSharesAsync(IMultiPartyNetworkSession multiPartySession, BitArray localShares)
    {
        foreach (var session in multiPartySession.RemotePartySessions)
            await session.Channel.WriteMessageAsync(new Message(localShares.ToBytes()));

        return localShares;
    }

    private static async Task<BitArray> ReceiveOutputRemoteSharesAsync(ITwoPartyNetworkSession session, SecureProgramContext context)
    {
        var message = await session.Channel.ReadMessageAsync();
        return BitArray.FromBytes(message.ToBuffer(), context.GetOutputContext().TotalNumberOfNonConstantBits);
    }

    private static SecureProgramOutput CreateSecureProgramOutput(SecureProgramContext context, BitArray[] perPartyOutputShares)
    {
        var outputBits = BitArray.FromXor(perPartyOutputShares);
        var outputBitsReader = outputBits.GetReader();
        return new SecureProgramOutput(
            context.GetOutputContext().ExpressionDescriptions
                .ToDictionary(
                    description => description.Output,
                    description => (description.Expression, description.ReadBits(outputBitsReader))
                )
        );
    }

    private record PerPartyShares(Party Party, BitArray Shares);
}
