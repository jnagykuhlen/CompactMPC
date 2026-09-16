using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;

namespace CompactMPC.Protocol.Internal;

public class TestSecureProgramRunner<TInput>
{
    private readonly List<Role> _partyRoles = new();
    private readonly List<TInput> _partyInputs = new();

    public TestSecureProgramRunner<TInput> WithParty(TInput input, Role role)
    {
        _partyInputs.Add(input);
        _partyRoles.Add(role);
        return this;
    }

    public TestSecureProgramRunner<TInput> WithParty(TInput input) => WithParty(input, Role.Default);

    public async Task<TOutput[]> RunAsync<TOutput>(Func<TInput, SecretSharingSecureComputation, Task<TOutput>> action)
    {
        var sessionDescription = new MultiPartySessionDescription(_partyRoles);
        var tasks = new Task<TOutput>[sessionDescription.NumberOfParties];

        await LocalNetworkRunner.RunMultiPartyNetworkAsync(
            sessionDescription,
            sessionInfo =>
                tasks[sessionInfo.LocalPartyIndex] = action(
                    _partyInputs[sessionInfo.LocalPartyIndex],
                    CreateSecureComputation(sessionInfo.Session)
                )
        );

        return await Task.WhenAll(tasks);
    }

    private static SecretSharingSecureComputation CreateSecureComputation(IMultiPartyNetworkSession session) =>
        new(session, new SecurityParameters(47, 23, 4, 1, 1));
}

public static class TestSecureProgramRunner
{
    public static TestSecureProgramRunner<TInput> WithParties<TInput>(params TInput[] inputs)
    {
        var runner = new TestSecureProgramRunner<TInput>();

        foreach (var input in inputs)
            runner.WithParty(input);

        return runner;
    }

    public static TestSecureProgramRunner<TInput> WithParty<TInput>(TInput input, Role role) =>
        new TestSecureProgramRunner<TInput>().WithParty(input, role);

    public static TestSecureProgramRunner<TInput> WithParty<TInput>(TInput input) => WithParty(input, Role.Default);
}
