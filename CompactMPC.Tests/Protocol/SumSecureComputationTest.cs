using System.Threading.Tasks;
using AwesomeAssertions;
using CompactMPC.Protocol.Expressions;
using CompactMPC.Protocol.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Protocol;

[TestClass]
public class SumSecureComputationTest
{
    [DataRow(new[] { 1, 2 }, 3)]
    [DataRow(new[] { 1, 2, 3 }, 6)]
    [DataRow(new[] { 1, 2, 3, 4 }, 10)]
    [DataRow(new[] { 1, 2, 3, 4, 5 }, 15)]
    [TestMethod]
    public Task TestSumSecureProgram(int[] inputs, int expectedOutput) =>
        TestSecureProgramRunner.RunAsync(inputs, async (input, secureComputation) =>
            {
                var output = await secureComputation.Run<SumSecureProgram>()
                    .WithInput(program => program.Input, input)
                    .EvaluateOutputAsync(program => program.Output);

                output.Should().Be(expectedOutput);
            }
        );

    [TestMethod]
    public void TestSumSecureProgramStatistics()
    {
        var compiledProgram = new SumSecureProgram().Compile(new MultiPartySessionDescription(3));

        var statistics = compiledProgram.GetStatistics();

        statistics.TotalNumberOfGates.Should().Be(41);
        statistics.NumberOfAndGates.Should().Be(8);
        statistics.NumberOfXorGates.Should().Be(33);
        statistics.NumberOfNotGates.Should().Be(0);
        statistics.MultiplicativeDepth.Should().Be(4);
    }

    private class SumSecureProgram : SecureProgram
    {
        public Input<SecureInteger> Input { get; } = SecureInteger.Input(10);
        public Output<SecureInteger> Output { get; } = SecureInteger.Output();

        protected override void Compile(ISecureProgramContext context)
        {
            var allInputs = context.Share(Input);
            context.Reveal(Output, SecureInteger.Sum(allInputs));
        }
    }
}
