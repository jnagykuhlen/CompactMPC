using System;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.Protocol.New;

public class SecretSharingSecureComputation(IMultiPartyNetworkSession multiPartySession, IMultiplicativeSharing multiplicativeSharing)
{
    public IMultiPartyNetworkSession MultiPartySession { get; } = multiPartySession;

    public async Task<SecureProgramOutput> RunAsync(SecureProgram program, SecureProgramInput input)
    {
        throw new NotImplementedException();
    }
    
    public SecureComputationRun<TProgram> Run<TProgram>(TProgram program) where TProgram : SecureProgram =>
        new(this, program);
}
