using System;
using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;

namespace CompactMPC.Protocol.Internal;

public static class TestSecureProgramRunner
{
    public static Task RunAsync<T>(T[] input, Func<T, SecretSharingSecureComputation, Task> action)
    {
        var securityParameters = new SecurityParameters(47, 23, 4, 1, 1);
        var obliviousTransfer = new NaorPinkasObliviousTransfer(securityParameters);
        var multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(obliviousTransfer);

        return LocalNetworkRunner.RunMultiPartyNetworkAsync(
            input.Length,
            sessionInfo => action(
                input[sessionInfo.LocalPartyIndex],
                new SecretSharingSecureComputation(sessionInfo.Session, multiplicativeSharing)
            )
        );
    }
}
