using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;

namespace CompactMPC.Protocol.Internal;

public class TestSecureProgramRunner<T>
{
    private readonly List<Role> _partyRoles = new();
    private readonly List<T> _partyInputs = new();

    public TestSecureProgramRunner<T> WithParty(T input, Role role)
    {
        _partyInputs.Add(input);
        _partyRoles.Add(role);
        return this;
    }

    public TestSecureProgramRunner<T> WithParty(T input) => WithParty(input, Role.Default);

    public Task RunAsync(Func<T, SecretSharingSecureComputation, Task> action) =>
        LocalNetworkRunner.RunMultiPartyNetworkAsync(
            new MultiPartySessionDescription(_partyRoles),
            sessionInfo => action(
                _partyInputs[sessionInfo.LocalPartyIndex],
                new SecretSharingSecureComputation(
                    sessionInfo.Session,
                    new SecurityParameters(47, 23, 4, 1, 1)
                )
            )
        );
}

public static class TestSecureProgramRunner
{
    public static TestSecureProgramRunner<T> WithParties<T>(params T[] inputs)
    {
        var runner = new TestSecureProgramRunner<T>();

        foreach (var input in inputs)
            runner.WithParty(input);

        return runner;
    }
    
    public static TestSecureProgramRunner<T> WithParty<T>(T input, Role role) =>
        new TestSecureProgramRunner<T>().WithParty(input, role);

    public static TestSecureProgramRunner<T> WithParty<T>(T input) => WithParty(input, Role.Default);
}
