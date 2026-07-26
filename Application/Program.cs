using System;
using System.Threading.Tasks;
using CompactMPC;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;
using CompactMPC.Protocol.Expressions;

string[] inputs = [
    "111101",
    "110101",
    "010110"
];

await LocalNetworkRunner.RunMultiPartyNetworkAsync(3, PerformSecureComputation);

async Task PerformSecureComputation(IMultiPartyNetworkSession networkSession, int partyIndex)
{
    var localInput = BitArray.FromBinaryString(inputs[partyIndex]);

    var obliviousTransfer = new NaorPinkasObliviousTransfer(
        new SecurityParameters(47, 23, 4, 1, 1)
    );

    var multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(obliviousTransfer);

    var secureComputation = new SecretSharingSecureComputation(
        networkSession,
        multiplicativeSharing
    );

    var (intersection, counter) = await secureComputation.Run(new SetIntersectionSecureProgram(localInput.Length))
        .WithInput(program => program.Input, localInput)
        .EvaluateOutputsAsync(
            program => program.IntersectionOutput,
            program => program.CounterOutput
        );

    Console.WriteLine($"OUTPUT: Intersection {intersection}, Counter {counter}");
}

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
