using System.Threading.Tasks;
using AwesomeAssertions;
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
        public Input<SecureBitArray> Input { get; } = SecureBitArray.Input(4);
        public Output<SecureBitArray> Output { get; } = SecureBitArray.Output();

        public override void Compile(ISecureProgramContext context)
        {
            var allInputs = context.Share(Input);
            context.Reveal(Output, SecureBitArray.Xor(allInputs) & SecureBitArray.AllZeroes(4));
        }
    }
}
