using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using CompactMPC.Networking;
using CompactMPC.Protocol.Expressions;
using CompactMPC.Protocol.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Protocol;

[TestClass]
public class RolesSecureComputationTest
{
    [TestMethod]
    public Task TestRolesSecureProgram() =>
        TestSecureProgramRunner
            .WithParty(8, RolesSecureProgram.RoleAlice)
            .WithParty(7, RolesSecureProgram.RoleBob)
            .RunAsync((input, secureComputation) =>
                secureComputation.Run<RolesSecureProgram>()
                    .WithInput(program => program.Input, input)
                    .EvaluateOutputAsync(program => program.Output)
            )
            .AndThen(outputs =>
                outputs.Should().Equal(true, false)
            );

    [TestMethod]
    public async Task TestRolesSecureProgramWithNonUniqueRole()
    {
        var runActionAsync = () => TestSecureProgramRunner
            .WithParty(8, RolesSecureProgram.RoleAlice)
            .WithParty(7, RolesSecureProgram.RoleAlice)
            .RunAsync((input, secureComputation) =>
                secureComputation.Run<RolesSecureProgram>()
                    .WithInput(program => program.Input, input)
                    .EvaluateOutputsAsync()
            );

        await runActionAsync.Should().ThrowAsync<InvalidOperationException>();
    }

    private class RolesSecureProgram : SecureProgram
    {
        public static readonly Role RoleAlice = new("Alice");
        public static readonly Role RoleBob = new("Bob");

        public Input<SecureInteger> Input { get; } = SecureInteger.Input(10);
        public Output<SecureBoolean> Output { get; } = SecureBoolean.Output();

        protected override void Compile(ISecureProgramContext context)
        {
            var inputAlice = context.ShareSingle(Input, RoleAlice);
            var inputBob = context.ShareSingle(Input, RoleBob);
            context.Reveal(Output, inputAlice >= inputBob, RoleAlice);
            context.Reveal(Output, inputBob >= inputAlice, RoleBob);
        }
    }
}
