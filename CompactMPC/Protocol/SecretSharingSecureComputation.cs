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

    public Task<SecureProgramOutput> RunAsync(SecureProgram program, SecureProgramInput programInput) =>
        RunAsync(program.Compile(new MultiPartySessionDescription(session.RemotePartySessions.Count() + 1)), programInput);

    public async Task<SecureProgramOutput> RunAsync(CompiledSecureProgram compiledProgram, SecureProgramInput programInput)
    {
        var sessionCompiledProgram = new SessionCompiledSecureProgram(compiledProgram, _session);
        var perPartyInputShares = await ExchangeInputSharesAsync(programInput, sessionCompiledProgram);

        var circuitEvaluator = new SecretSharingAsyncBatchCircuitEvaluator(_session, multiplicativeSharing);
        var evaluationResult = await new ForwardCircuitEvaluation<Bit>(circuitEvaluator).ExecuteAsync(
            GetInputWireValues(perPartyInputShares, sessionCompiledProgram),
            GetOutputWires(sessionCompiledProgram)
        );

        var perPartyOutputShares = await ExchangeOutputSharesAsync(evaluationResult, sessionCompiledProgram);

        return CreateSecureProgramOutput(perPartyOutputShares, sessionCompiledProgram);
    }

    private Task<PerPartyShares[]> ExchangeInputSharesAsync(SecureProgramInput programInput, SessionCompiledSecureProgram sessionCompiledProgram)
    {
        var localShares = sessionCompiledProgram.GetInputContext(session.LocalParty).GetInputBits(programInput);
        return _session.SendAndReceiveAsync(
            multiPartySession => SendInputRemoteSharesAsync(multiPartySession, localShares),
            twoPartySession => ReceiveInputLocalSharesAsync(twoPartySession, sessionCompiledProgram)
        );
    }

    private static async Task<PerPartyShares> SendInputRemoteSharesAsync(
        IMultiPartyNetworkSession multiPartyNetworkSession,
        BitArray localShares)
    {
        foreach (var remotePartySession in multiPartyNetworkSession.RemotePartySessions)
        {
            var remoteShares = RandomNumberGenerator.GetBits(localShares.Length);
            localShares.Xor(remoteShares);
            await remotePartySession.Channel.WriteMessageAsync(new Message(remoteShares.ToBytes()));
        }

        return new PerPartyShares(multiPartyNetworkSession.LocalParty, localShares);
    }

    private static async Task<PerPartyShares> ReceiveInputLocalSharesAsync(ITwoPartyNetworkSession twoPartySession, SessionCompiledSecureProgram sessionCompiledProgram)
    {
        var message = await twoPartySession.Channel.ReadMessageAsync();
        return new PerPartyShares(
            twoPartySession.RemoteParty,
            BitArray.FromBytes(
                message.ToBuffer(),
                sessionCompiledProgram.GetInputContext(twoPartySession.RemoteParty).TotalNumberOfBits
            )
        );
    }

    private IEnumerable<WireValue<Bit>> GetInputWireValues(PerPartyShares[] perPartyInputShares, SessionCompiledSecureProgram sessionCompiledProgram) =>
        perPartyInputShares.SelectMany(shares =>
            sessionCompiledProgram.GetInputContext(shares.Party).ExpressionDescriptions
                .SelectMany(description => description.Expression.Wires)
                .Select((wire, index) => new WireValue<Bit>(wire, shares.Shares[index]))
        );

    private IEnumerable<Wire> GetOutputWires(SessionCompiledSecureProgram sessionCompiledProgram) =>
        sessionCompiledProgram.OutputContext.NonConstantWires;

    private Task<BitArray[]> ExchangeOutputSharesAsync(ForwardCircuitEvaluationResult<Bit> evaluationResult, SessionCompiledSecureProgram sessionCompiledProgram)
    {
        var localShares = new BitArray(sessionCompiledProgram.OutputContext.NonConstantWires.Select(evaluationResult.Value).ToArray());
        return _session.SendAndReceiveAsync(
            multiPartySession => SendOutputLocalSharesAsync(multiPartySession, localShares),
            twoPartySession => ReceiveOutputRemoteSharesAsync(twoPartySession, sessionCompiledProgram)
        );
    }

    private static async Task<BitArray> SendOutputLocalSharesAsync(IMultiPartyNetworkSession multiPartySession, BitArray localShares)
    {
        foreach (var remotePartySession in multiPartySession.RemotePartySessions)
            await remotePartySession.Channel.WriteMessageAsync(new Message(localShares.ToBytes()));

        return localShares;
    }

    private static async Task<BitArray> ReceiveOutputRemoteSharesAsync(ITwoPartyNetworkSession twoPartySession, SessionCompiledSecureProgram sessionCompiledProgram)
    {
        var message = await twoPartySession.Channel.ReadMessageAsync();
        return BitArray.FromBytes(message.ToBuffer(), sessionCompiledProgram.OutputContext.TotalNumberOfNonConstantBits);
    }

    private SecureProgramOutput CreateSecureProgramOutput(BitArray[] perPartyOutputShares, SessionCompiledSecureProgram sessionCompiledProgram)
    {
        var outputBits = BitArray.FromXor(perPartyOutputShares);
        var outputBitsReader = outputBits.GetReader();
        return new SecureProgramOutput(
            sessionCompiledProgram.OutputContext.ExpressionDescriptions
                .ToDictionary(
                    description => description.Output,
                    description => (description.Expression, description.ReadBits(outputBitsReader))
                )
        );
    }

    private record PerPartyShares(Party Party, BitArray Shares);
}
