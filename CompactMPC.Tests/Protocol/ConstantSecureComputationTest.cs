using System.Threading.Tasks;
using AwesomeAssertions;
using CompactMPC.Protocol.Expressions;
using CompactMPC.Protocol.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Protocol;

[TestClass]
public class ConstantSecureComputationTest
{
    [TestMethod]
    public Task TestConstantSecureProgram() =>
        TestSecureProgramRunner
            .WithParty(BitArray.FromBinaryString("1010"))
            .WithParty(BitArray.FromBinaryString("1101"))
            .RunAsync(async (input, secureComputation) =>
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

        protected override void Compile(ISecureProgramContext context)
        {
            var allInputs = context.Share(Input);
            context.Reveal(Output, SecureBitArray.Xor(allInputs).And(SecureBitArray.AllZeroes(4)));
        }
    }
}
