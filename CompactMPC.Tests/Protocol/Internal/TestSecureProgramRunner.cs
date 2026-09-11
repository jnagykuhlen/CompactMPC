using System;
using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;

namespace CompactMPC.Protocol.Internal;

public static class TestSecureProgramRunner
{
    public static Task RunAsync<T>(T[] input, Func<T, SecretSharingSecureComputation, Task> action) =>
        LocalNetworkRunner.RunMultiPartyNetworkAsync(
            input.Length,
            sessionInfo => action(
                input[sessionInfo.LocalPartyIndex],
                new SecretSharingSecureComputation(
                    sessionInfo.Session,
                    new SecurityParameters(47, 23, 4, 1, 1)
                )
            )
        );
}
