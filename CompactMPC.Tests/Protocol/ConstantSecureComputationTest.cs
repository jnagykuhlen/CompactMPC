using System.Threading.Tasks;
using AwesomeAssertions;
using CompactMPC.Expressions;
using CompactMPC.Protocol.Internal;
using CompactMPC.Protocol.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Protocol;

[TestClass]
public class ConstantSecureComputationTest
{
    [TestMethod]
    public Task TestConstantSecureProgram() =>
        TestSecureProgramRunner.RunAsync(
            [BitArray.FromBinaryString("1010"), BitArray.FromBinaryString("1101")],
            async (input, secureComputation) =>
            {
                var output = await secureComputation.Run<ConstantSecureProgram>()
                    .WithInput(program => program.Input, input)
                    .EvaluateOutputAsync(program => program.Output);

                output.ToBinaryString().Should().Be("0000");
            }
        );

    private class ConstantSecureProgram : SecureProgram
    {
        public Input<BitArrayExpression> Input { get; } = BitArrayExpression.Input(4);
        public Output<BitArrayExpression> Output { get; } = BitArrayExpression.Output();

        public override void Compile(ISecureProgramContext context)
        {
            var allInputs = context.Share(Input);
            context.Reveal(Output, BitArrayExpression.Xor(allInputs) & BitArrayExpression.AllZeroes(4));
        }
    }
}
