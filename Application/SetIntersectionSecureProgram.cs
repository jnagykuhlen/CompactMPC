using CompactMPC.Protocol;
using CompactMPC.Protocol.Expressions;

namespace CompactMPC.Application;

public class SetIntersectionSecureProgram(int numberOfBits) : SecureProgram
{
    public Input<SecureBitArray> Input { get; } = SecureBitArray.Input(numberOfBits);
    public Output<SecureBitArray> IntersectionOutput { get; } = SecureBitArray.Output();
    public Output<SecureInteger> CounterOutput { get; } = SecureInteger.Output();

    public override void Compile(ISecureProgramContext context)
    {
        var allInputs = context.Share(Input);
        var intersection = SecureBitArray.And(allInputs);

        var counter = SecureInteger.Zero;
        for (var i = 0; i < numberOfBits; ++i)
            counter += SecureInteger.FromBoolean(intersection.IsBitSet(i));

        context.Reveal(IntersectionOutput, intersection);
        context.Reveal(CounterOutput, counter);
    }
}
