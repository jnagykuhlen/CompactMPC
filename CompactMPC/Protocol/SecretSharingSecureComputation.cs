using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Circuits;
using CompactMPC.Collections;
using CompactMPC.Cryptography;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public class SecretSharingSecureComputation(IMultiPartyNetworkSession session, IMultiplicativeSharing multiplicativeSharing)
{
    private readonly OrderedMultiPartyNetworkSession _session = new(session);

    public SecretSharingSecureComputation(IMultiPartyNetworkSession session, SecurityParameters securityParameters) :
        this(session, new ObliviousTransferMultiplicativeSharing(new NaorPinkasObliviousTransfer(securityParameters))) { }

    public SecureComputationRun<TProgram> Run<TProgram>(TProgram program) where TProgram : SecureProgram =>
        new(this, program);

    public SecureComputationRun<TProgram> Run<TProgram>() where TProgram : SecureProgram, new() =>
        new(this, new TProgram());

    public Task<SecureProgramOutput> RunAsync(SecureProgram program, SecureProgramInput programInput) =>
        RunAsync(program.Compile(_session.CreateSessionDescription()), programInput);

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
        var localShares = sessionCompiledProgram.GetContext(session.LocalParty).InputContext.GetBits(programInput);
        return _session.SendAndReceiveAsync(
            multiPartySession => SendInputRemoteSharesAsync(multiPartySession, localShares),
            twoPartySession => ReceiveInputLocalSharesAsync(twoPartySession, sessionCompiledProgram)
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

    private static async Task<PerPartyShares> ReceiveInputLocalSharesAsync(ITwoPartyNetworkSession twoPartySession, SessionCompiledSecureProgram sessionCompiledProgram)
    {
        var message = await twoPartySession.Channel.ReadMessageAsync();
        return new PerPartyShares(
            twoPartySession.RemoteParty,
            BitArray.FromBytes(
                message.ToBuffer(),
                sessionCompiledProgram.GetContext(twoPartySession.RemoteParty).InputContext.TotalNumberOfBits
            )
        );
    }

    private IEnumerable<WireValue<Bit>> GetInputWireValues(PerPartyShares[] perPartyInputShares, SessionCompiledSecureProgram sessionCompiledProgram) =>
        perPartyInputShares.SelectMany(shares =>
            sessionCompiledProgram.GetContext(shares.Party).InputContext.Wires
                .Select((wire, index) => new WireValue<Bit>(wire, shares.Shares[index]))
        );

    private IEnumerable<Wire> GetOutputWires(SessionCompiledSecureProgram sessionCompiledProgram) =>
        _session.OrderedParties
            .SelectMany(party => sessionCompiledProgram.GetContext(party).OutputContext.NonConstantWires)
            .Distinct();

    private Task<BitArray[]> ExchangeOutputSharesAsync(ForwardCircuitEvaluationResult<Bit> evaluationResult, SessionCompiledSecureProgram sessionCompiledProgram)
    {
        return _session.SendAndReceiveAsync(
            multiPartySession => SendOutputLocalSharesAsync(multiPartySession, sessionCompiledProgram, evaluationResult),
            twoPartySession => ReceiveOutputRemoteSharesAsync(twoPartySession, sessionCompiledProgram)
        );
    }

    private async Task<BitArray> SendOutputLocalSharesAsync(IMultiPartyNetworkSession multiPartySession, SessionCompiledSecureProgram sessionCompiledProgram, ForwardCircuitEvaluationResult<Bit> evaluationResult)
    {
        foreach (var remotePartySession in multiPartySession.RemotePartySessions)
        {
            var localShares = new BitArray(
                sessionCompiledProgram.GetContext(remotePartySession.RemoteParty).OutputContext.NonConstantWires
                    .Select(evaluationResult.Value)
                    .ToArray()
            );
            
            await remotePartySession.Channel.WriteMessageAsync(new Message(localShares.ToBytes()));
        }

        return new BitArray(
            sessionCompiledProgram.GetContext(_session.LocalParty).OutputContext.NonConstantWires
                .Select(evaluationResult.Value)
                .ToArray()
        );
    }

    private async Task<BitArray> ReceiveOutputRemoteSharesAsync(ITwoPartyNetworkSession twoPartySession, SessionCompiledSecureProgram sessionCompiledProgram)
    {
        var message = await twoPartySession.Channel.ReadMessageAsync();
        return BitArray.FromBytes(message.ToBuffer(), sessionCompiledProgram.GetContext(_session.LocalParty).OutputContext.TotalNumberOfNonConstantBits);
    }

    private SecureProgramOutput CreateSecureProgramOutput(BitArray[] perPartyOutputShares, SessionCompiledSecureProgram sessionCompiledProgram)
    {
        var outputBits = BitArray.FromXor(perPartyOutputShares);
        var outputBitsReader = outputBits.GetReader();
        return new SecureProgramOutput(
            sessionCompiledProgram.GetContext(_session.LocalParty).OutputContext.ExpressionDescriptions
                .ToDictionary(
                    description => description.Output,
                    description => (description.Expression, description.ReadBits(outputBitsReader))
                )
        );
    }

    private record PerPartyShares(Party Party, BitArray Shares);
}
