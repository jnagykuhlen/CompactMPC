using System.Threading.Tasks;
using CompactMPC.Expressions;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Protocol;

[TestClass]
public class SecureComputationTest
{
    private static readonly int[] Inputs = [1, 2, 3, 4, 5];

    [TestMethod]
    public Task TestTwoPartySetIntersection() => RunSecureComputationParties(2, 3);

    [TestMethod]
    public Task TestThreePartySetIntersection() => RunSecureComputationParties(3, 6);

    [TestMethod]
    public Task TestFourPartySetIntersection() => RunSecureComputationParties(4, 10);

    [TestMethod]
    public Task TestFivePartySetIntersection() => RunSecureComputationParties(5, 15);

    private static Task RunSecureComputationParties(int numberOfParties, int expectedOutput) =>
        LocalNetworkRunner.RunMultiPartyNetwork(
            numberOfParties,
            (networkSession, partyIndex) => PerformSecureComputation(networkSession, partyIndex, expectedOutput)
        );

    private static async Task PerformSecureComputation(IMultiPartyNetworkSession networkSession, int partyIndex, int expectedOutput)
    {
        var localInput = Inputs[partyIndex];

        var obliviousTransfer = new NaorPinkasObliviousTransfer(
            new SecurityParameters(47, 23, 4, 1, 1)
        );

        var multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(obliviousTransfer);

        var secureComputation = new SecretSharingSecureComputation(
            networkSession,
            multiplicativeSharing
        );

        var output = await secureComputation.Run(new SumSecureProgram())
            .WithInput(program => program.Input, localInput)
            .EvaluateOutputAsync(program => program.Output);

        output.Should().Be(expectedOutput);
    }
}

public class SumSecureProgram : SecureProgram
{
    public Input<IntegerExpression> Input { get; } = IntegerExpression.Input(10);
    public Output<IntegerExpression> Output { get; } = IntegerExpression.Output();

    public override void Compile(ISecureProgramContext context)
    {
        var allInputs = context.Share(Input);
        context.Reveal(Output, IntegerExpression.Sum(allInputs));
    }
}
